namespace NetIOKit.Core;

public readonly record struct InMemorySessionRunnerMetricsSnapshot(
    long BytesReceived,
    long MessagesDispatched,
    long ReconnectAttempts,
    long ReconnectSuccess,
    long ReceiveFailures);
