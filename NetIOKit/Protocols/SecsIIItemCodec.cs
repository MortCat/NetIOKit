using System.Buffers;
using System.Text;

namespace NetIOKit.Protocols;

/// <summary>
/// Minimal SECS-II item codec for v0/v1 scaffold.
/// This codec only handles the smallest practical subset:
/// - ASCII item
/// - U4 item
///
/// It is intentionally limited to lock compatibility and enable incremental HSMS/SECS-II expansion.
/// </summary>
public sealed class SecsIIItemCodec
{
    private const int HeaderLength = 5; // 1 byte format + 4 bytes length
    private readonly int _maxPayloadBytes;

    public SecsIIItemCodec(int maxPayloadBytes = 1024 * 1024)
    {
        if (maxPayloadBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes));
        }

        _maxPayloadBytes = maxPayloadBytes;
    }

    public byte[] Encode(SecsIIItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item switch
        {
            SecsIIItem.Ascii ascii => EncodeAscii(ascii),
            SecsIIItem.U4 u4 => EncodeU4(u4),
            _ => throw new InvalidDataException($"Unsupported SECS-II item type: {item.GetType().Name}")
        };
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out SecsIIItem? item, out int consumedBytes)
    {
        item = null;
        consumedBytes = 0;

        if (source.Length < HeaderLength)
        {
            return false;
        }

        Span<byte> header = stackalloc byte[HeaderLength];
        source.Slice(0, HeaderLength).CopyTo(header);

        var format = header[0];
        var length = BitConverter.ToInt32(header[1..]);
        if (length < 0 || length > _maxPayloadBytes)
        {
            throw new InvalidDataException($"Invalid SECS-II payload length: {length}");
        }

        var packetLength = HeaderLength + length;
        if (source.Length < packetLength)
        {
            return false;
        }

        var payload = source.Slice(HeaderLength, length).ToArray();
        item = format switch
        {
            SecsIIItem.AsciiCode => new SecsIIItem.Ascii(Encoding.ASCII.GetString(payload)),
            SecsIIItem.U4Code => DecodeU4(payload),
            _ => throw new InvalidDataException($"Unsupported SECS-II format code: 0x{format:X2}")
        };

        consumedBytes = packetLength;
        return true;
    }

    private byte[] EncodeAscii(SecsIIItem.Ascii item)
    {
        var payload = Encoding.ASCII.GetBytes(item.Value ?? string.Empty);
        return BuildFrame(SecsIIItem.AsciiCode, payload);
    }

    private byte[] EncodeU4(SecsIIItem.U4 item)
    {
        var payload = BitConverter.GetBytes(item.Value);
        return BuildFrame(SecsIIItem.U4Code, payload);
    }

    private byte[] BuildFrame(byte format, byte[] payload)
    {
        if (payload.Length > _maxPayloadBytes)
        {
            throw new InvalidDataException($"SECS-II payload exceeds max {_maxPayloadBytes} bytes.");
        }

        var output = new byte[HeaderLength + payload.Length];
        output[0] = format;
        Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, output, 1, 4);
        Buffer.BlockCopy(payload, 0, output, HeaderLength, payload.Length);
        return output;
    }

    private static SecsIIItem.U4 DecodeU4(byte[] payload)
    {
        if (payload.Length != 4)
        {
            throw new InvalidDataException($"U4 payload must be 4 bytes, actual={payload.Length}");
        }

        var value = BitConverter.ToUInt32(payload);
        return new SecsIIItem.U4(value);
    }
}
