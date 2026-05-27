namespace NetIOKit.Protocols;

/// <summary>
/// Unified HSMS frame wrapper for control/data multiplexing.
/// </summary>
public readonly record struct HsmsUnifiedFrame(
    HsmsFrameKind Kind,
    HsmsControlMessage? Control,
    HsmsDataMessage? Data)
{
    public static HsmsUnifiedFrame FromControl(HsmsControlMessage message) =>
        new(HsmsFrameKind.Control, message, null);

    public static HsmsUnifiedFrame FromData(HsmsDataMessage message) =>
        new(HsmsFrameKind.Data, null, message);
}
