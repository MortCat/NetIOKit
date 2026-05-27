using NetIOKit.Core;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Tests;

public sealed class HsmsControlDataCoexistenceIntegrationTests
{
    [Fact]
    public async Task ControlAndDataSessions_CanRunIndependently_OnSeparateTransports()
    {
        await using var controlTransport = new FakeTransport();
        await using var dataTransport = new FakeTransport();
        await controlTransport.OpenAsync();
        await dataTransport.OpenAsync();

        var hsmsSession = new NetIOHsmsSession(controlTransport);
        var dataCodec = new HsmsDataMessageCodec();
        HsmsDataMessage? dataReceived = null;
        var dataRunner = new SessionRunner<HsmsDataMessage>(
            dataTransport,
            dataCodec,
            new DefaultPipelineStrategy<HsmsDataMessage>((m, _) =>
            {
                dataReceived = m;
                return ValueTask.CompletedTask;
            }));

        controlTransport.EnqueueIncoming(new HsmsControlMessageCodec().Encode(
            new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100)));
        dataTransport.EnqueueIncoming(dataCodec.Encode(
            new HsmsDataMessage(1, 200, 1, 1, false, new byte[] { 0x01 })));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(180));
        await Task.WhenAll(hsmsSession.RunAsync(cts.Token), dataRunner.RunAsync(cts.Token));

        Assert.Equal(HsmsConnectionState.Selected, hsmsSession.State);
        Assert.True(dataReceived.HasValue);
        Assert.Equal((byte)1, dataReceived.Value.Stream);
    }
}
