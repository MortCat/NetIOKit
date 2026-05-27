using NetIOKit.Core;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Tests;

public class SessionRunnerMetricsTests
{
    [Fact]
    public async Task Metrics_AreUpdated_ForDispatchAndBytes()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        var metrics = new InMemorySessionRunnerMetrics();
        var strategy = new DefaultPipelineStrategy<byte[]>((_, _) => ValueTask.CompletedTask);
        var runner = new SessionRunner<byte[]>(transport, parser, strategy, metrics: metrics);

        var p1 = parser.Encode(new byte[] { 1, 2, 3 });
        var p2 = parser.Encode(new byte[] { 9 });
        transport.EnqueueIncoming(p1, p2);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(80));
        await runner.RunAsync(cts.Token);

        Assert.Equal(2, metrics.MessagesDispatched);
        Assert.True(metrics.BytesReceived >= p1.Length + p2.Length);
    }

    [Fact]
    public async Task Metrics_RecordReconnectAttemptAndFailure()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new FakeTransport();
        await transport.OpenAsync();
        transport.ThrowOnNextReceive = true;

        var metrics = new InMemorySessionRunnerMetrics();
        var strategy = new DefaultPipelineStrategy<byte[]>((_, _) => ValueTask.CompletedTask);
        var policy = new ExponentialBackoffReconnectPolicy(maxAttempts: 1, baseDelay: TimeSpan.FromMilliseconds(1), maxDelay: TimeSpan.FromMilliseconds(1), jitterRatio: 0);
        var runner = new SessionRunner<byte[]>(transport, parser, strategy, reconnectPolicy: policy, metrics: metrics);

        var p = parser.Encode(new byte[] { 7 });
        transport.EnqueueIncoming(p);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(80));
        await runner.RunAsync(cts.Token);

        Assert.True(metrics.ReceiveFailures >= 1);
        Assert.True(metrics.ReconnectAttempts >= 1);
        Assert.True(metrics.ReconnectSuccess >= 1);
    }
}
