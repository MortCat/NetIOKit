using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class HsmsControlMessageCodecTests
{
    [Fact]
    public void EncodeDecode_SelectRequest_RoundTrip()
    {
        var codec = new HsmsControlMessageCodec();
        var input = new HsmsControlMessage(
            HsmsControlMessageType.SelectRequest,
            SessionId: 10,
            SystemBytes: 0x01020304,
            Status: 0);

        var bytes = codec.Encode(input);
        var ok = codec.TryDecode(bytes, out var decoded, out var consumed);

        Assert.True(ok);
        Assert.Equal(bytes.Length, consumed);
        Assert.Equal(input, decoded);
    }

    [Fact]
    public void TryDecode_Incomplete_ReturnsFalse()
    {
        var codec = new HsmsControlMessageCodec();
        var incomplete = new byte[] { 0x08, 0x00, 0x00 };

        var ok = codec.TryDecode(incomplete, out var decoded, out var consumed);

        Assert.False(ok);
        Assert.Equal(default, decoded);
        Assert.Equal(0, consumed);
    }

    [Fact]
    public void Decode_InvalidLength_Throws()
    {
        var codec = new HsmsControlMessageCodec();
        var invalid = new byte[]
        {
            0x07, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        Assert.Throws<InvalidDataException>(() => codec.TryDecode(invalid, out _, out _));
    }
}
