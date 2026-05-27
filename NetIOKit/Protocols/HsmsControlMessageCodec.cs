using System.Buffers;
using NetIOKit.Abstractions;

namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS control codec scaffold for Select/Linktest subset.
///
/// Wire format:
/// [0..3]  TotalLength (Int32 little-endian), fixed value 8
/// [4]     MessageType
/// [5..6]  SessionId (UInt16 little-endian)
/// [7..10] SystemBytes (UInt32 little-endian)
/// [11]    Status
/// </summary>
public sealed class HsmsControlMessageCodec : IProtocolCodec<HsmsControlMessage>
{
    private const int HeaderLength = 4;
    private const int BodyLength = 8;

    public byte[] Encode(HsmsControlMessage message)
    {
        ValidateType(message.Type);

        var output = new byte[HeaderLength + BodyLength];
        Buffer.BlockCopy(BitConverter.GetBytes(BodyLength), 0, output, 0, HeaderLength);
        output[4] = (byte)message.Type;
        Buffer.BlockCopy(BitConverter.GetBytes(message.SessionId), 0, output, 5, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(message.SystemBytes), 0, output, 7, 4);
        output[11] = message.Status;
        return output;
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out HsmsControlMessage message, out int consumedBytes)
    {
        message = default;
        consumedBytes = 0;

        if (source.Length < HeaderLength)
        {
            return false;
        }

        Span<byte> lenBytes = stackalloc byte[HeaderLength];
        source.Slice(0, HeaderLength).CopyTo(lenBytes);
        var length = BitConverter.ToInt32(lenBytes);

        if (length != BodyLength)
        {
            throw new InvalidDataException($"Invalid HSMS control body length: {length}");
        }

        var total = HeaderLength + BodyLength;
        if (source.Length < total)
        {
            return false;
        }

        var body = source.Slice(HeaderLength, BodyLength).ToArray();
        var type = (HsmsControlMessageType)body[0];
        ValidateType(type);

        var sessionId = BitConverter.ToUInt16(body, 1);
        var systemBytes = BitConverter.ToUInt32(body, 3);
        var status = body[7];

        message = new HsmsControlMessage(type, sessionId, systemBytes, status);
        consumedBytes = total;
        return true;
    }

    private static void ValidateType(HsmsControlMessageType type)
    {
        if (type is not HsmsControlMessageType.SelectRequest
            and not HsmsControlMessageType.SelectResponse
            and not HsmsControlMessageType.LinktestRequest
            and not HsmsControlMessageType.LinktestResponse)
        {
            throw new InvalidDataException($"Unsupported HSMS control type: {type}");
        }
    }
}
