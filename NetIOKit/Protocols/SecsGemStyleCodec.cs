using System.Buffers;
using System.Text;
using NetIOKit.Abstractions;

namespace NetIOKit.Protocols;

/// <summary>
/// SECS/GEM-style adapter skeleton codec.
///
/// Wire format (skeleton, for contract/compat tests):
/// [0..3]   TotalLength (Int32 Little-Endian), where TotalLength = 3 + BodyLength
/// [4]      Stream (S)
/// [5]      Function (F)
/// [6]      Flags bit0 => WaitBit (W)
/// [7..N-1] Body UTF-8 bytes
///
/// This is NOT a full HSMS/SECS-II implementation yet.
/// It is a minimal scaffold to lock API/behavior contracts before full protocol expansion.
/// </summary>
public sealed class SecsGemStyleCodec : IProtocolCodec<SecsGemMessage>
{
    private const int HeaderLength = 4;
    private const int MetaLength = 3;
    private readonly int _maxBodyBytes;

    public SecsGemStyleCodec(int maxBodyBytes = 1024 * 1024)
    {
        if (maxBodyBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBodyBytes));
        }

        _maxBodyBytes = maxBodyBytes;
    }

    public byte[] Encode(SecsGemMessage message)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(message.Body ?? string.Empty);
        if (bodyBytes.Length > _maxBodyBytes)
        {
            throw new InvalidDataException($"SECS/GEM body exceeds max {_maxBodyBytes} bytes.");
        }

        var totalLength = MetaLength + bodyBytes.Length;
        var output = new byte[HeaderLength + totalLength];

        Buffer.BlockCopy(BitConverter.GetBytes(totalLength), 0, output, 0, HeaderLength);
        output[4] = message.Stream;
        output[5] = message.Function;
        output[6] = (byte)(message.WaitBit ? 0b0000_0001 : 0);
        Buffer.BlockCopy(bodyBytes, 0, output, HeaderLength + MetaLength, bodyBytes.Length);

        return output;
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out SecsGemMessage message, out int consumedBytes)
    {
        message = default;
        consumedBytes = 0;

        if (source.Length < HeaderLength)
        {
            return false;
        }

        Span<byte> lenBytes = stackalloc byte[HeaderLength];
        source.Slice(0, HeaderLength).CopyTo(lenBytes);
        var totalLength = BitConverter.ToInt32(lenBytes);

        if (totalLength < MetaLength)
        {
            throw new InvalidDataException($"Invalid SECS/GEM total length: {totalLength}.");
        }

        var bodyLength = totalLength - MetaLength;
        if (bodyLength > _maxBodyBytes)
        {
            throw new InvalidDataException($"SECS/GEM body exceeds max {_maxBodyBytes} bytes.");
        }

        var packetLength = HeaderLength + totalLength;
        if (source.Length < packetLength)
        {
            return false;
        }

        Span<byte> meta = stackalloc byte[MetaLength];
        source.Slice(HeaderLength, MetaLength).CopyTo(meta);

        var bodyBytes = source.Slice(HeaderLength + MetaLength, bodyLength).ToArray();
        message = new SecsGemMessage(
            Stream: meta[0],
            Function: meta[1],
            WaitBit: (meta[2] & 0b0000_0001) == 0b0000_0001,
            Body: Encoding.UTF8.GetString(bodyBytes));

        consumedBytes = packetLength;
        return true;
    }
}
