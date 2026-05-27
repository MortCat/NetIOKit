using NetIOKit.Abstractions;
using NetIOKit.Core;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Tests;

public class SessionRunnerReconnectTests
{
    [Fact]
    public async Task Runner_Reconnects_AfterTransientFailure_AndContinues()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new FakeTransport();
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
            maxDelay: TimeSpan.FromMilliseconds(5),
            jitterRatio: 0,
            random: new Random(42));

        var runner = new SessionRunner<byte[]>(transport, parser, strategy, reconnectPolicy: policy);

        transport.ThrowOnNextReceive = true;
        var packet = parser.Encode(new byte[] { 4, 4, 4 });
        transport.EnqueueIncoming(packet);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(80));
        await runner.RunAsync(cts.Token);

        Assert.Single(received);
        Assert.Equal(new byte[] { 4, 4, 4 }, received[0]);
    }

    [Fact]
    public async Task Runner_StopsAfterMaxAttempts()
    {
        var parser = new LengthPrefixedPacketParser();
        await using var transport = new AlwaysFailTransport();
        await transport.OpenAsync();

        var strategy = new DefaultPipelineStrategy<byte[]>((_, _) => ValueTask.CompletedTask);
        var policy = new ExponentialBackoffReconnectPolicy(
            maxAttempts: 2,
            baseDelay: TimeSpan.FromMilliseconds(1),
            maxDelay: TimeSpan.FromMilliseconds(2),
            jitterRatio: 0,
            random: new Random(11));

        var runner = new SessionRunner<byte[]>(transport, parser, strategy, reconnectPolicy: policy);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAsync<NetIOException>(() => runner.RunAsync(cts.Token));
        Assert.True(transport.OpenCount >= 2);
    }

    private sealed class AlwaysFailTransport : ITransport
    {
        public int OpenCount { get; private set; }

        public ValueTask OpenAsync(CancellationToken cancellationToken = default)
        {
            OpenCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(new IOException("always fail"));

        public ValueTask CloseAsync(CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
