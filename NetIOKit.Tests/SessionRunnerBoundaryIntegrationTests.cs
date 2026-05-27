using NetIOKit.Abstractions;
using NetIOKit.Core;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Tests;

public sealed class SessionRunnerBoundaryIntegrationTests
{
    [Fact]
    public async Task RunAsync_CancelledBeforeStart_ExitsWithoutDispatch()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new ControlledFailTransport();
        await transport.OpenAsync();

        var dispatchCount = 0;
        var strategy = new DefaultPipelineStrategy<byte[]>((_, _) =>
        {
            dispatchCount++;
            return ValueTask.CompletedTask;
        });

        var runner = new SessionRunner<byte[]>(transport, parser, strategy);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await runner.RunAsync(cts.Token);

        Assert.Equal(0, dispatchCount);
        Assert.Equal(0, transport.ReceiveCalls);
    }

    [Fact]
    public async Task RunAsync_DisconnectThenCancel_DoesNotThrow()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new ControlledFailTransport();
        await transport.OpenAsync();

        var strategy = new DefaultPipelineStrategy<byte[]>((_, _) => ValueTask.CompletedTask);
        var policy = new ExponentialBackoffReconnectPolicy(
            maxAttempts: 5,
            baseDelay: TimeSpan.FromMilliseconds(5),
            maxDelay: TimeSpan.FromMilliseconds(5),
            jitterRatio: 0,
            random: new Random(7));

        var runner = new SessionRunner<byte[]>(transport, parser, strategy, reconnectPolicy: policy);

        transport.FailuresRemaining = 1;

        using var cts = new CancellationTokenSource();
        var task = runner.RunAsync(cts.Token);
        await Task.Delay(5);
        cts.Cancel();

        await task;
        Assert.True(transport.OpenCount >= 1);
        Assert.True(transport.CloseCount >= 1);
    }

    [Fact]
    public async Task RunAsync_ReconnectRace_RecoversAndDispatchesOnce()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new ControlledFailTransport();
        await transport.OpenAsync();

        var received = new List<byte[]>();
        var strategy = new DefaultPipelineStrategy<byte[]>((m, _) =>
        {
            received.Add(m);
            return ValueTask.CompletedTask;
        });

        var policy = new ExponentialBackoffReconnectPolicy(
            maxAttempts: 3,
            baseDelay: TimeSpan.FromMilliseconds(1),
            maxDelay: TimeSpan.FromMilliseconds(2),
            jitterRatio: 0,
            random: new Random(9));

        var metrics = new InMemorySessionRunnerMetrics();
        var runner = new SessionRunner<byte[]>(transport, parser, strategy, reconnectPolicy: policy, metrics: metrics);

        transport.FailuresRemaining = 1;
        var packet = parser.Encode(new byte[] { 7, 8, 9 });

        // Queue payload slightly after first failure to simulate reconnect race window.
        _ = Task.Run(async () =>
        {
            await Task.Delay(8);
            transport.EnqueueIncoming(packet);
        });

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        await runner.RunAsync(cts.Token);

        Assert.Single(received);
        Assert.Equal(new byte[] { 7, 8, 9 }, received[0]);
        Assert.True(metrics.ReconnectAttempts >= 1);
        Assert.True(metrics.ReconnectSuccess >= 1);
    }

    private sealed class ControlledFailTransport : ITransport
    {
        private readonly Queue<byte[]> _segments = new();
        private readonly SemaphoreSlim _signal = new(0);

        public int FailuresRemaining { get; set; }
        public int OpenCount { get; private set; }
        public int CloseCount { get; private set; }
        public int ReceiveCalls { get; private set; }
        public bool IsOpen { get; private set; }

        public ValueTask OpenAsync(CancellationToken cancellationToken = default)
        {
            IsOpen = true;
            OpenCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReceiveCalls++;
            if (!IsOpen)
            {
                return 0;
            }

            if (FailuresRemaining > 0)
            {
                FailuresRemaining--;
                throw new IOException("Simulated disconnect for boundary test.");
            }

            await _signal.WaitAsync(cancellationToken);

            if (_segments.Count == 0)
            {
                return 0;
            }

            var segment = _segments.Dequeue();
            segment.AsSpan().CopyTo(buffer.Span);
            return segment.Length;
        }

        public ValueTask CloseAsync(CancellationToken cancellationToken = default)
        {
            IsOpen = false;
            CloseCount++;
            return ValueTask.CompletedTask;
        }

        public void EnqueueIncoming(byte[] segment)
        {
            _segments.Enqueue(segment);
            _signal.Release();
        }

        public async ValueTask DisposeAsync()
        {
            await CloseAsync();
            _signal.Dispose();
        }
    }
}
