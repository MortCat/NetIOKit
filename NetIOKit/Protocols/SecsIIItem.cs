namespace NetIOKit.Protocols;

/// <summary>
/// Minimal SECS-II item model subset.
/// Current supported item formats:
/// - ASCII string
/// - U4 (UInt32)
///
/// Binary layout (subset):
/// [0]      FormatCode (0x41 ASCII, 0xB1 U4)
/// [1..4]   Length (Int32 little-endian)
/// [5..N-1] Payload bytes
/// </summary>
public abstract record SecsIIItem
{
    public const byte AsciiCode = 0x41;
    public const byte U4Code = 0xB1;

    public sealed record Ascii(string Value) : SecsIIItem;
    public sealed record U4(uint Value) : SecsIIItem;
}
