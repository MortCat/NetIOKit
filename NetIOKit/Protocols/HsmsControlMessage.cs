namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS control message contract.
/// SessionId and SystemBytes are preserved for request/response correlation.
/// Status is primarily used by response messages.
/// </summary>
public readonly record struct HsmsControlMessage(
    HsmsControlMessageType Type,
    ushort SessionId,
    uint SystemBytes,
    byte Status = 0);
