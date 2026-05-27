namespace NetIOKit.Abstractions;

public static class ErrorCodes
{
    public const string TransportConnectionFailed = "NIO-TRN-001";
    public const string SessionHandshakeTimeout = "NIO-SES-001";
    public const string ProtocolInvalidFrameLength = "NIO-PRT-001";
    public const string StrategyQueueOverflow = "NIO-STG-001";
}
