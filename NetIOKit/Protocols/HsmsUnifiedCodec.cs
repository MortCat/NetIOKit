using System.Buffers;
using NetIOKit.Abstractions;

namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS unified codec skeleton.
/// Adds 1-byte kind prefix before inner frame bytes:
/// [0]      FrameKind (0=Control, 1=Data)
/// [1..N-1] Inner encoded frame from control/data codec
///
/// Readability/maintenance note:
/// - Control/Data inner frames keep their own codec contracts.
/// - Unified envelope only does frame-kind routing.
/// </summary>
public sealed class HsmsUnifiedCodec : IProtocolCodec<HsmsUnifiedFrame>
{
    private const int KindPrefixLength = 1;

    private readonly HsmsControlMessageCodec _control = new();
    private readonly HsmsDataMessageCodec _data = new();

    public byte[] Encode(HsmsUnifiedFrame message)
    {
        return message.Kind switch
        {
            HsmsFrameKind.Control when message.Control.HasValue
                => Prefix(HsmsFrameKind.Control, _control.Encode(message.Control.Value)),
            HsmsFrameKind.Data when message.Data.HasValue
                => Prefix(HsmsFrameKind.Data, _data.Encode(message.Data.Value)),
            _ => throw new InvalidDataException("HSMS unified frame missing payload for selected kind.")
        };
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out HsmsUnifiedFrame message, out int consumedBytes)
    {
        message = default;
        consumedBytes = 0;

        if (source.Length < KindPrefixLength)
        {
            return false;
        }

        var kindByte = source.Slice(0, KindPrefixLength).First.Span[0];
        var inner = source.Slice(KindPrefixLength);

        return kindByte switch
        {
            (byte)HsmsFrameKind.Control => TryDecodeControl(inner, out message, out consumedBytes),
            (byte)HsmsFrameKind.Data => TryDecodeData(inner, out message, out consumedBytes),
            _ => throw new InvalidDataException($"Unsupported HSMS frame kind: {kindByte}")
        };
    }

    private bool TryDecodeControl(ReadOnlySequence<byte> inner, out HsmsUnifiedFrame message, out int consumedBytes)
    {
        message = default;
        consumedBytes = 0;

        if (!_control.TryDecode(inner, out var control, out var innerConsumed))
        {
            return false;
        }

        consumedBytes = KindPrefixLength + innerConsumed;
        message = HsmsUnifiedFrame.FromControl(control);
        return true;
    }

    private bool TryDecodeData(ReadOnlySequence<byte> inner, out HsmsUnifiedFrame message, out int consumedBytes)
    {
        message = default;
        consumedBytes = 0;

        if (!_data.TryDecode(inner, out var data, out var innerConsumed))
        {
            return false;
        }

        consumedBytes = KindPrefixLength + innerConsumed;
        message = HsmsUnifiedFrame.FromData(data);
        return true;
    }

    private static byte[] Prefix(HsmsFrameKind kind, byte[] payload)
    {
        var output = new byte[KindPrefixLength + payload.Length];
        output[0] = (byte)kind;
        Buffer.BlockCopy(payload, 0, output, KindPrefixLength, payload.Length);
        return output;
    }
}
