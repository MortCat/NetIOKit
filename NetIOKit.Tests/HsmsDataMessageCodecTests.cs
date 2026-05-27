using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class HsmsDataMessageCodecTests
{
    [Fact]
    public void EncodeDecode_RoundTrip()
    {
        var codec = new HsmsDataMessageCodec();
        var input = new HsmsDataMessage(5, 0x10203040, 1, 13, true, new byte[] { 0xAA, 0xBB });

        var bytes = codec.Encode(input);
        var ok = codec.TryDecode(bytes, out var decoded, out var consumed);

        Assert.True(ok);
        Assert.Equal(bytes.Length, consumed);
        Assert.Equal(input.SessionId, decoded.SessionId);
        Assert.Equal(input.SystemBytes, decoded.SystemBytes);
        Assert.Equal(input.Stream, decoded.Stream);
        Assert.Equal(input.Function, decoded.Function);
        Assert.Equal(input.WaitBit, decoded.WaitBit);
        Assert.Equal(input.Payload, decoded.Payload);
    }
}
