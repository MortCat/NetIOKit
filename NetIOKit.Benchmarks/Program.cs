using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using NetIOKit.Protocols;

var options = BenchmarkOptions.Parse(args);
var benchmark = new LoopbackBenchmark(options);

Console.WriteLine("NetIOKit Benchmark/Soak Harness v1");
Console.WriteLine($"Mode={options.Mode}, Duration={options.DurationSeconds}s, Payload={options.PayloadSizeBytes} bytes, Clients={options.ClientCount}");

var result = options.Mode switch
{
    BenchmarkMode.Throughput => await benchmark.RunThroughputAsync(),
    BenchmarkMode.Soak => await benchmark.RunSoakAsync(),
    _ => throw new ArgumentOutOfRangeException()
};

Console.WriteLine("--- Result ---");
Console.WriteLine($"TotalMessages={result.TotalMessages}");
Console.WriteLine($"TotalBytes={result.TotalBytes}");
Console.WriteLine($"MessagesPerSecond={result.MessagesPerSecond:F2}");
Console.WriteLine($"MegabytesPerSecond={result.MegabytesPerSecond:F2}");
Console.WriteLine($"LatencyP50Ms={result.LatencyP50Ms:F3}");
Console.WriteLine($"LatencyP95Ms={result.LatencyP95Ms:F3}");
Console.WriteLine($"LatencyP99Ms={result.LatencyP99Ms:F3}");
Console.WriteLine($"Duration={result.Duration}");

internal enum BenchmarkMode
{
    Throughput,
    Soak
}

internal sealed record BenchmarkOptions(BenchmarkMode Mode, int DurationSeconds, int PayloadSizeBytes, int ClientCount)
{
    public static BenchmarkOptions Parse(string[] args)
    {
        var mode = BenchmarkMode.Throughput;
        var duration = 15;
        var payload = 256;
        var clients = 4;

        foreach (var arg in args)
        {
            var parts = arg.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;
            switch (parts[0].ToLowerInvariant())
            {
                case "mode":
                    mode = parts[1].Equals("soak", StringComparison.OrdinalIgnoreCase) ? BenchmarkMode.Soak : BenchmarkMode.Throughput;
                    break;
                case "duration":
                    if (int.TryParse(parts[1], out var d) && d > 0) duration = d;
                    break;
                case "payload":
                    if (int.TryParse(parts[1], out var p) && p > 0) payload = p;
                    break;
                case "clients":
                    if (int.TryParse(parts[1], out var c) && c > 0) clients = c;
                    break;
            }
        }

        return new BenchmarkOptions(mode, duration, payload, clients);
    }
}

internal sealed record BenchmarkResult(
    long TotalMessages,
    long TotalBytes,
    double MessagesPerSecond,
    double MegabytesPerSecond,
    double LatencyP50Ms,
    double LatencyP95Ms,
    double LatencyP99Ms,
    TimeSpan Duration);

internal sealed class LoopbackBenchmark
{
    private readonly BenchmarkOptions _options;

    public LoopbackBenchmark(BenchmarkOptions options) => _options = options;

    public Task<BenchmarkResult> RunSoakAsync() => RunThroughputAsync();

    public async Task<BenchmarkResult> RunThroughputAsync()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.DurationSeconds));

        var server = RunEchoServerAsync(listener, cts.Token);
        var clientTasks = Enumerable.Range(0, _options.ClientCount)
            .Select(_ => RunClientAsync(port, cts.Token))
            .ToArray();

        await Task.WhenAll(clientTasks);
        await cts.CancelAsync();
        listener.Stop();
        await server;

        var totalMessages = clientTasks.Sum(t => t.Result.Messages);
        var totalBytes = clientTasks.Sum(t => t.Result.Bytes);
        var latencies = clientTasks.SelectMany(t => t.Result.LatencySamplesMs).OrderBy(v => v).ToArray();

        var seconds = _options.DurationSeconds;
        return new BenchmarkResult(
            totalMessages,
            totalBytes,
            totalMessages / (double)seconds,
            totalBytes / 1024d / 1024d / seconds,
            Percentile(latencies, 0.50),
            Percentile(latencies, 0.95),
            Percentile(latencies, 0.99),
            TimeSpan.FromSeconds(seconds));
    }

    private static double Percentile(double[] samples, double percentile)
    {
        if (samples.Length == 0) return 0;
        var index = (int)Math.Ceiling(percentile * samples.Length) - 1;
        index = Math.Clamp(index, 0, samples.Length - 1);
        return samples[index];
    }

    private async Task RunEchoServerAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        var connections = new List<Task>();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(5, cancellationToken);
                    continue;
                }

                var socket = await listener.AcceptSocketAsync(cancellationToken);
                connections.Add(HandleConnectionAsync(socket, cancellationToken));
            }
        }
        catch (OperationCanceledException)
        {
        }

        await Task.WhenAll(connections);
    }

    private async Task HandleConnectionAsync(Socket socket, CancellationToken cancellationToken)
    {
        using var _ = socket;
        var stream = new NetworkStream(socket, ownsSocket: false);
        var buffer = new byte[16 * 1024];

        while (!cancellationToken.IsCancellationRequested)
        {
            int read;
            try
            {
                read = await stream.ReadAsync(buffer, cancellationToken);
            }
            catch
            {
                break;
            }

            if (read <= 0) break;
            await stream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
    }

    private async Task<(long Messages, long Bytes, List<double> LatencySamplesMs)> RunClientAsync(int port, CancellationToken cancellationToken)
    {
        var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);
        using var _ = client;

        var stream = client.GetStream();
        var codec = new LengthPrefixedPacketParser();
        var payload = new byte[_options.PayloadSizeBytes];
        Random.Shared.NextBytes(payload);
        var frame = codec.Encode(payload);

        var receiveBuffer = new byte[frame.Length];
        long messages = 0;
        long bytes = 0;
        var latencies = new List<double>(capacity: 4096);

        while (!cancellationToken.IsCancellationRequested)
        {
            var sw = Stopwatch.StartNew();
            await stream.WriteAsync(frame, cancellationToken);

            var offset = 0;
            while (offset < frame.Length)
            {
                var read = await stream.ReadAsync(receiveBuffer.AsMemory(offset, frame.Length - offset), cancellationToken);
                if (read <= 0) return (messages, bytes, latencies);
                offset += read;
            }

            sw.Stop();
            messages++;
            bytes += payload.Length;
            latencies.Add(sw.Elapsed.TotalMilliseconds);
        }

        return (messages, bytes, latencies);
    }
}
