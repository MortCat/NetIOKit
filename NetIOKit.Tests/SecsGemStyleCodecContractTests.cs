using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class SecsGemStyleCodecContractTests
{
    [Fact]
    public void RoundTrip_EncodeDecode_PreservesContractFields()
    {
        var codec = new SecsGemStyleCodec();
        var input = new SecsGemMessage(Stream: 1, Function: 13, WaitBit: true, Body: "HELLO");

        var frame = codec.Encode(input);
        var ok = codec.TryDecode(frame, out var decoded, out var consumed);

        Assert.True(ok);
        Assert.Equal(frame.Length, consumed);
        Assert.Equal((byte)1, decoded.Stream);
        Assert.Equal((byte)13, decoded.Function);
        Assert.True(decoded.WaitBit);
        Assert.Equal("HELLO", decoded.Body);
    }

    [Fact]
    public void TryDecode_IncompleteFrame_ReturnsFalseAndConsumesZero()
    {
        var codec = new SecsGemStyleCodec();
        var frame = codec.Encode(new SecsGemMessage(2, 41, false, "ABC"));
        var partial = frame[..(frame.Length - 1)];

        var ok = codec.TryDecode(partial, out var decoded, out var consumed);

        Assert.False(ok);
        Assert.Equal(default, decoded);
        Assert.Equal(0, consumed);
    }

    [Fact]
    public void Decode_InvalidLength_Throws()
    {
        var codec = new SecsGemStyleCodec();
        var invalid = new byte[]
        {
            0x01, 0x00, 0x00, 0x00, // totalLength=1 (smaller than meta length=3)
            0x00
        };

        Assert.Throws<InvalidDataException>(() => codec.TryDecode(invalid, out _, out _));
    }
}
