using System.Net;
using System.Net.Sockets;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Core;

public static class MinimalClientServerDemo
{
    public static async Task<MinimalClientServerDemoResult> RunAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var parser = new LengthPrefixedPacketParser();
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        try
        {
            var serverTask = RunServerOnceAsync(listener, parser, cancellationToken);
            var clientTask = RunClientOnceAsync(port, parser, message, cancellationToken);

            await Task.WhenAll(serverTask, clientTask).ConfigureAwait(false);

            return new MinimalClientServerDemoResult(serverTask.Result, clientTask.Result);
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async Task<string> RunServerOnceAsync(TcpListener listener, LengthPrefixedPacketParser parser, CancellationToken cancellationToken)
    {
        using var socket = await listener.AcceptSocketAsync(cancellationToken).ConfigureAwait(false);
        await using var stream = new NetworkStream(socket, ownsSocket: false);

        var buffer = new byte[4096];
        var readBuffer = new ProtocolReadBuffer<byte[]>(parser);

        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read <= 0)
            {
                break;
            }

            readBuffer.Append(buffer.AsSpan(0, read));
            while (readBuffer.TryRead(out var payload))
            {
                var received = System.Text.Encoding.UTF8.GetString(payload);
                var response = $"ACK:{received}";
                var frame = parser.Encode(System.Text.Encoding.UTF8.GetBytes(response));
                await stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
                return received;
            }
        }

        throw new InvalidOperationException("Server did not receive a full frame.");
    }

    private static async Task<string> RunClientOnceAsync(int port, LengthPrefixedPacketParser parser, string message, CancellationToken cancellationToken)
    {
        var endpoint = new TcpEndpoint("127.0.0.1", port);
        await using var transport = new TcpTransport(endpoint);
        await transport.OpenAsync(cancellationToken).ConfigureAwait(false);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var strategy = new DefaultPipelineStrategy<byte[]>((payload, _) =>
        {
            var ack = System.Text.Encoding.UTF8.GetString(payload);
            tcs.TrySetResult(ack);
            return ValueTask.CompletedTask;
        });

        var runner = new SessionRunner<byte[]>(transport, parser, strategy);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var runnerTask = runner.RunAsync(linkedCts.Token);

        var frame = parser.Encode(System.Text.Encoding.UTF8.GetBytes(message));
        await transport.SendAsync(frame, cancellationToken).ConfigureAwait(false);

        var ackMessage = await tcs.Task.ConfigureAwait(false);
        linkedCts.Cancel();
        await runnerTask.ConfigureAwait(false);

        await transport.CloseAsync(cancellationToken).ConfigureAwait(false);
        return ackMessage;
    }
}

public readonly record struct MinimalClientServerDemoResult(string ServerReceivedMessage, string ClientReceivedAck);
