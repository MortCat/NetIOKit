using NetIOKit.Core;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Tests;

public sealed class HsmsControlMessageIntegrationTests
{
    [Fact]
    public async Task SessionRunner_Dispatches_LinktestRequest_ControlMessage()
    {
        var codec = new HsmsControlMessageCodec();
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        HsmsControlMessage? received = null;
        var strategy = new DefaultPipelineStrategy<HsmsControlMessage>((m, _) =>
        {
            received = m;
            return ValueTask.CompletedTask;
        });

        var runner = new SessionRunner<HsmsControlMessage>(transport, codec, strategy);

        var message = new HsmsControlMessage(HsmsControlMessageType.LinktestRequest, 22, 0x11223344, 0);
        transport.EnqueueIncoming(codec.Encode(message));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        await runner.RunAsync(cts.Token);

        Assert.True(received.HasValue);
        Assert.Equal(message, received.Value);
    }
}
