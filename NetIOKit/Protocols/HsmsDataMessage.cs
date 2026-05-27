namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS data message scaffold.
/// Payload uses SECS-II item bytes (or adapter-specific binary) as opaque data for now.
/// </summary>
public readonly record struct HsmsDataMessage(
    ushort SessionId,
    uint SystemBytes,
    byte Stream,
    byte Function,
    bool WaitBit,
    byte[] Payload);
