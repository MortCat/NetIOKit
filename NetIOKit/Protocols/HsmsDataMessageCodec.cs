using System.Buffers;
using NetIOKit.Abstractions;

namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS data-message codec scaffold.
/// Wire format:
/// [0..3]  BodyLength (Int32 little-endian), BodyLength = 8 + PayloadLength
/// [4..5]  SessionId (UInt16 little-endian)
/// [6..9]  SystemBytes (UInt32 little-endian)
/// [10]    Stream
/// [11]    Function
/// [12]    Flags (bit0 = WaitBit)
/// [13..N] Payload bytes
/// </summary>
public sealed class HsmsDataMessageCodec : IProtocolCodec<HsmsDataMessage>
{
    private const int HeaderLength = 4;
    private const int MetaLength = 9;
    private readonly int _maxPayloadBytes;

    public HsmsDataMessageCodec(int maxPayloadBytes = 1024 * 1024)
    {
        if (maxPayloadBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes));
        }

        _maxPayloadBytes = maxPayloadBytes;
    }

    public byte[] Encode(HsmsDataMessage message)
    {
        var payload = message.Payload ?? Array.Empty<byte>();
        if (payload.Length > _maxPayloadBytes)
        {
            throw new InvalidDataException($"HSMS data payload exceeds max {_maxPayloadBytes} bytes.");
        }

        var bodyLength = MetaLength + payload.Length;
        var output = new byte[HeaderLength + bodyLength];

        Buffer.BlockCopy(BitConverter.GetBytes(bodyLength), 0, output, 0, HeaderLength);
        Buffer.BlockCopy(BitConverter.GetBytes(message.SessionId), 0, output, 4, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(message.SystemBytes), 0, output, 6, 4);
        output[10] = message.Stream;
        output[11] = message.Function;
        output[12] = (byte)(message.WaitBit ? 0b0000_0001 : 0);
        Buffer.BlockCopy(payload, 0, output, HeaderLength + MetaLength, payload.Length);

        return output;
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out HsmsDataMessage message, out int consumedBytes)
    {
        message = default;
        consumedBytes = 0;

        if (source.Length < HeaderLength)
        {
            return false;
        }

        Span<byte> lenBytes = stackalloc byte[HeaderLength];
        source.Slice(0, HeaderLength).CopyTo(lenBytes);
        var bodyLength = BitConverter.ToInt32(lenBytes);
        if (bodyLength < MetaLength)
        {
            throw new InvalidDataException($"Invalid HSMS data body length: {bodyLength}");
        }

        var payloadLength = bodyLength - MetaLength;
        if (payloadLength > _maxPayloadBytes)
        {
            throw new InvalidDataException($"HSMS data payload exceeds max {_maxPayloadBytes} bytes.");
        }

        var total = HeaderLength + bodyLength;
        if (source.Length < total)
        {
            return false;
        }

        var body = source.Slice(HeaderLength, bodyLength).ToArray();
        var sessionId = BitConverter.ToUInt16(body, 0);
        var systemBytes = BitConverter.ToUInt32(body, 2);
        var stream = body[6];
        var function = body[7];
        var waitBit = (body[8] & 0b0000_0001) == 1;
        var payload = body.AsSpan(MetaLength).ToArray();

        message = new HsmsDataMessage(sessionId, systemBytes, stream, function, waitBit, payload);
        consumedBytes = total;
        return true;
    }
}
