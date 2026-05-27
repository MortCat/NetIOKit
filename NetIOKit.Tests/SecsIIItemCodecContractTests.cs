using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class SecsIIItemCodecContractTests
{
    [Fact]
    public void EncodeDecode_Ascii_RoundTrip()
    {
        var codec = new SecsIIItemCodec();
        var item = new SecsIIItem.Ascii("HELLO");

        var encoded = codec.Encode(item);
        var ok = codec.TryDecode(encoded, out var decoded, out var consumed);

        Assert.True(ok);
        Assert.Equal(encoded.Length, consumed);
        var ascii = Assert.IsType<SecsIIItem.Ascii>(decoded);
        Assert.Equal("HELLO", ascii.Value);
    }

    [Fact]
    public void EncodeDecode_U4_RoundTrip()
    {
        var codec = new SecsIIItemCodec();
        var item = new SecsIIItem.U4(123456u);

        var encoded = codec.Encode(item);
        var ok = codec.TryDecode(encoded, out var decoded, out var consumed);

        Assert.True(ok);
        Assert.Equal(encoded.Length, consumed);
        var u4 = Assert.IsType<SecsIIItem.U4>(decoded);
        Assert.Equal(123456u, u4.Value);
    }

    [Fact]
    public void TryDecode_IncompleteHeader_ReturnsFalse()
    {
        var codec = new SecsIIItemCodec();
        var incomplete = new byte[] { SecsIIItem.AsciiCode, 0x01 };

        var ok = codec.TryDecode(incomplete, out var decoded, out var consumed);

        Assert.False(ok);
        Assert.Null(decoded);
        Assert.Equal(0, consumed);
    }

    [Fact]
    public void TryDecode_InvalidFormat_Throws()
    {
        var codec = new SecsIIItemCodec();
        var invalid = new byte[]
        {
            0xFF, // unsupported format
            0x00,0x00,0x00,0x00
        };

        Assert.Throws<InvalidDataException>(() => codec.TryDecode(invalid, out _, out _));
    }
}
