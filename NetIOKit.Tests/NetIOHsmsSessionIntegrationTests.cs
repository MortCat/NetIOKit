using NetIOKit.Core;
using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class NetIOHsmsSessionIntegrationTests
{
    [Fact]
    public async Task RunAsync_SelectThenLinktest_TransitionsAndDispatches()
    {
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        var received = new List<HsmsControlMessage>();
        var session = new NetIOHsmsSession(transport, msg => received.Add(msg));
        var codec = new HsmsControlMessageCodec();

        transport.EnqueueIncoming(
            codec.Encode(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100)),
            codec.Encode(new HsmsControlMessage(HsmsControlMessageType.LinktestRequest, 1, 101)));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(180));
        await session.RunAsync(cts.Token);

        Assert.Equal(HsmsConnectionState.Selected, session.State);
        Assert.Equal(2, received.Count);
    }

    [Fact]
    public async Task RunAsync_LinktestBeforeSelect_ThrowsInvalidOperation()
    {
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        var session = new NetIOHsmsSession(transport);
        var codec = new HsmsControlMessageCodec();

        transport.EnqueueIncoming(codec.Encode(
            new HsmsControlMessage(HsmsControlMessageType.LinktestRequest, 1, 200)));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(180));
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.RunAsync(cts.Token));
    }
}
