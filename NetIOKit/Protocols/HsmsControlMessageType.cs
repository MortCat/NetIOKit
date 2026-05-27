namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS control message subset for scaffold:
/// - Select.req / Select.rsp
/// - Linktest.req / Linktest.rsp
/// </summary>
public enum HsmsControlMessageType : byte
{
    SelectRequest = 0x01,
    SelectResponse = 0x02,
    LinktestRequest = 0x05,
    LinktestResponse = 0x06
}
