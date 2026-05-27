using System.Buffers;
using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public class LengthPrefixedPacketParserTests
{
    [Fact]
    public void EncodeDecode_RoundTrip_Succeeds()
    {
        var parser = new LengthPrefixedPacketParser();
        var payload = new byte[] { 1, 2, 3, 4, 5 };

        var encoded = parser.Encode(payload);
        var ok = parser.TryDecode(new ReadOnlySequence<byte>(encoded), out var decoded, out var consumed);

        Assert.True(ok);
        Assert.Equal(encoded.Length, consumed);
        Assert.Equal(payload, decoded);
    }

    [Fact]
    public void TryDecode_IncompleteHeader_ReturnsFalse()
    {
        var parser = new LengthPrefixedPacketParser();
        var data = new byte[] { 0x01, 0x00, 0x00 };

        var ok = parser.TryDecode(new ReadOnlySequence<byte>(data), out _, out var consumed);

        Assert.False(ok);
        Assert.Equal(0, consumed);
    }

    [Fact]
    public void TryDecode_InvalidLength_Throws()
    {
        var parser = new LengthPrefixedPacketParser(maxPayloadLength: 8);
        var header = BitConverter.GetBytes(999);

        Assert.Throws<InvalidDataException>(() =>
            parser.TryDecode(new ReadOnlySequence<byte>(header), out _, out _));
    }
}
