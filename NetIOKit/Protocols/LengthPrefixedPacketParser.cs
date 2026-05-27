using System.Buffers;
using NetIOKit.Abstractions;

namespace NetIOKit.Protocols;

/// <summary>
/// Packet format:
/// [0..3]    : payload length (Int32, little-endian)
/// [4..N-1]  : payload bytes
/// </summary>
public sealed class LengthPrefixedPacketParser : IProtocolCodec<byte[]>
{
    private const int HeaderLength = 4;
    private readonly int _maxPayloadLength;

    public LengthPrefixedPacketParser(int maxPayloadLength = 1024 * 1024)
    {
        if (maxPayloadLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadLength));
        }

        _maxPayloadLength = maxPayloadLength;
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out byte[] message, out int consumedBytes)
    {
        message = Array.Empty<byte>();
        consumedBytes = 0;

        if (source.Length < HeaderLength)
        {
            return false;
        }

        Span<byte> header = stackalloc byte[HeaderLength];
        source.Slice(0, HeaderLength).CopyTo(header);
        var payloadLength = BitConverter.ToInt32(header);

        if (payloadLength < 0 || payloadLength > _maxPayloadLength)
        {
            throw new InvalidDataException($"Invalid payload length: {payloadLength}.");
        }

        var packetLength = HeaderLength + payloadLength;
        if (source.Length < packetLength)
        {
            return false;
        }

        message = source.Slice(HeaderLength, payloadLength).ToArray();
        consumedBytes = packetLength;
        return true;
    }

    public byte[] Encode(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Length > _maxPayloadLength)
        {
            throw new InvalidDataException($"Payload exceeds max allowed length {_maxPayloadLength}.");
        }

        var payloadLengthBytes = BitConverter.GetBytes(message.Length);
        var output = new byte[HeaderLength + message.Length];

        Buffer.BlockCopy(payloadLengthBytes, 0, output, 0, HeaderLength);
        Buffer.BlockCopy(message, 0, output, HeaderLength, message.Length);

        return output;
    }
}
