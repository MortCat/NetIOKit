using NetIOKit.Core;
using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class NetIOHsmsUnifiedSessionIntegrationTests
{
    [Fact]
    public async Task UnifiedSession_DispatchesControlAndData_AndUpdatesState()
    {
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        var controls = new List<HsmsControlMessage>();
        var datas = new List<HsmsDataMessage>();

        var session = new NetIOHsmsUnifiedSession(
            transport,
            onControl: controls.Add,
            onData: datas.Add);

        var codec = new HsmsUnifiedCodec();
        transport.EnqueueIncoming(
            codec.Encode(HsmsUnifiedFrame.FromControl(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100))),
            codec.Encode(HsmsUnifiedFrame.FromData(new HsmsDataMessage(1, 101, 1, 1, false, new byte[] { 0x01, 0x02 }))));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(180));
        await session.RunAsync(cts.Token);

        Assert.Equal(HsmsConnectionState.Selected, session.State);
        Assert.Single(controls);
        Assert.Single(datas);
    }
}
