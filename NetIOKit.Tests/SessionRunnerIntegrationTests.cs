using NetIOKit.Core;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Tests;

public class SessionRunnerIntegrationTests
{
    [Fact]
    public async Task SessionRunner_FakeTransport_FragmentedFrames_DispatchesMessages()
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

        var runner = new SessionRunner<byte[]>(transport, parser, strategy);

        var p1 = parser.Encode(new byte[] { 1, 2 });
        var p2 = parser.Encode(new byte[] { 3, 4, 5 });
        transport.EnqueueIncoming(p1[..3], p1[3..], p2);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        await runner.RunAsync(cts.Token);

        Assert.Equal(2, received.Count);
        Assert.Equal(new byte[] { 1, 2 }, received[0]);
        Assert.Equal(new byte[] { 3, 4, 5 }, received[1]);
    }
}
