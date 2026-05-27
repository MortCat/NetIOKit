namespace NetIOKit.Protocols;

/// <summary>
/// Minimal SECS/GEM-style message contract used by adapter scaffold.
/// This is intentionally simplified for v0/v1 skeleton:
/// - Stream (S)
/// - Function (F)
/// - WaitBit (W)
/// - Body (UTF-8 text placeholder; real SECS-II item tree comes later)
/// </summary>
public readonly record struct SecsGemMessage(
    byte Stream,
    byte Function,
    bool WaitBit,
    string Body);
