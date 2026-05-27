using NetIOKit.Abstractions;

namespace NetIOKit.Core;

public sealed class InMemorySessionRunnerMetrics : ISessionRunnerMetrics
{
    private readonly long _createdAtUnixTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    private long _bytesReceived;
    private long _messagesDispatched;
    private long _reconnectAttempts;
    private long _reconnectSuccess;
    private long _receiveFailures;

    public long BytesReceived => Interlocked.Read(ref _bytesReceived);
    public long MessagesDispatched => Interlocked.Read(ref _messagesDispatched);
    public long ReconnectAttempts => Interlocked.Read(ref _reconnectAttempts);
    public long ReconnectSuccess => Interlocked.Read(ref _reconnectSuccess);
    public long ReceiveFailures => Interlocked.Read(ref _receiveFailures);

    public InMemorySessionRunnerMetricsSnapshot GetSnapshot() =>
        new(
            BytesReceived,
            MessagesDispatched,
            ReconnectAttempts,
            ReconnectSuccess,
            ReceiveFailures);

    public double GetMessagesPerSecond()
    {
        var elapsedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _createdAtUnixTimeMilliseconds;
        if (elapsedMs <= 0)
        {
            return 0;
        }

        return MessagesDispatched * 1000d / elapsedMs;
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _bytesReceived, 0);
        Interlocked.Exchange(ref _messagesDispatched, 0);
        Interlocked.Exchange(ref _reconnectAttempts, 0);
        Interlocked.Exchange(ref _reconnectSuccess, 0);
        Interlocked.Exchange(ref _receiveFailures, 0);
    }

    public void OnBytesReceived(int bytes)
    {
        if (bytes > 0)
        {
            Interlocked.Add(ref _bytesReceived, bytes);
        }
    }

    public void OnMessageDispatched() => Interlocked.Increment(ref _messagesDispatched);

    public void OnReconnectAttempt(int attempt, TimeSpan delay) => Interlocked.Increment(ref _reconnectAttempts);

    public void OnReconnectSuccess() => Interlocked.Increment(ref _reconnectSuccess);

    public void OnReceiveFailure(Exception exception) => Interlocked.Increment(ref _receiveFailures);
}
