using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public class TransportIntegrationTests
{
    [Fact]
    public async Task Fragmentation_CanReassembleSingleMessage()
    {
        var parser = new LengthPrefixedPacketParser();
        var readBuffer = new ProtocolReadBuffer<byte[]>(parser);
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        var packet = parser.Encode(new byte[] { 7, 8, 9, 10 });
        transport.EnqueueIncoming(packet[..2], packet[2..5], packet[5..]);

        var temp = new byte[16];
        for (var i = 0; i < 3; i++)
        {
            var n = await transport.ReceiveAsync(temp);
            readBuffer.Append(temp.AsSpan(0, n));
        }

        Assert.True(readBuffer.TryRead(out var msg));
        Assert.Equal(new byte[] { 7, 8, 9, 10 }, msg);
    }

    [Fact]
    public async Task StickyPackets_CanDecodeInOrder()
    {
        var parser = new LengthPrefixedPacketParser();
        var readBuffer = new ProtocolReadBuffer<byte[]>(parser);
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        var p1 = parser.Encode(new byte[] { 1 });
        var p2 = parser.Encode(new byte[] { 2, 2 });
        var sticky = new byte[p1.Length + p2.Length];
        Buffer.BlockCopy(p1, 0, sticky, 0, p1.Length);
        Buffer.BlockCopy(p2, 0, sticky, p1.Length, p2.Length);
        transport.EnqueueIncoming(sticky);

        var temp = new byte[64];
        var n = await transport.ReceiveAsync(temp);
        readBuffer.Append(temp.AsSpan(0, n));

        Assert.True(readBuffer.TryRead(out var m1));
        Assert.True(readBuffer.TryRead(out var m2));
        Assert.Equal(new byte[] { 1 }, m1);
        Assert.Equal(new byte[] { 2, 2 }, m2);
    }

    [Fact]
    public async Task SimulatedDisconnect_CanReopenAndContinue()
    {
        var parser = new LengthPrefixedPacketParser();
        var readBuffer = new ProtocolReadBuffer<byte[]>(parser);
        await using var transport = new FakeTransport();
        await transport.OpenAsync();

        transport.ThrowOnNextReceive = true;
        var temp = new byte[32];
        await Assert.ThrowsAsync<IOException>(async () => await transport.ReceiveAsync(temp));

        await transport.CloseAsync();
        await transport.OpenAsync();

        var packet = parser.Encode(new byte[] { 3, 3, 3 });
        transport.EnqueueIncoming(packet);
        var n = await transport.ReceiveAsync(temp);
        readBuffer.Append(temp.AsSpan(0, n));

        Assert.True(readBuffer.TryRead(out var msg));
        Assert.Equal(new byte[] { 3, 3, 3 }, msg);
    }
}
