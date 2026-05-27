using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public class ProtocolReadBufferTests
{
    [Fact]
    public void ReadBuffer_PartialHeaderThenPayload_CanReadCompleteMessage()
    {
        var parser = new LengthPrefixedPacketParser();
        var buffer = new ProtocolReadBuffer<byte[]>(parser);
        var packet = parser.Encode(new byte[] { 10, 20, 30 });

        buffer.Append(packet.AsSpan(0, 2));
        Assert.False(buffer.TryRead(out _));

        buffer.Append(packet.AsSpan(2));
        var ok = buffer.TryRead(out var message);

        Assert.True(ok);
        Assert.Equal(new byte[] { 10, 20, 30 }, message);
        Assert.Equal(0, buffer.BufferedLength);
    }

    [Fact]
    public void ReadBuffer_MultiplePacketsInSingleAppend_ReadsInOrder()
    {
        var parser = new LengthPrefixedPacketParser();
        var buffer = new ProtocolReadBuffer<byte[]>(parser);

        var packet1 = parser.Encode(new byte[] { 1, 1 });
        var packet2 = parser.Encode(new byte[] { 2, 2, 2 });

        var combined = new byte[packet1.Length + packet2.Length];
        Buffer.BlockCopy(packet1, 0, combined, 0, packet1.Length);
        Buffer.BlockCopy(packet2, 0, combined, packet1.Length, packet2.Length);

        buffer.Append(combined);

        Assert.True(buffer.TryRead(out var m1));
        Assert.Equal(new byte[] { 1, 1 }, m1);

        Assert.True(buffer.TryRead(out var m2));
        Assert.Equal(new byte[] { 2, 2, 2 }, m2);

        Assert.False(buffer.TryRead(out _));
        Assert.Equal(0, buffer.BufferedLength);
    }
}
