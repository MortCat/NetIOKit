using NetIOKit.Abstractions;

namespace NetIOKit.Core;

public sealed class NullSessionRunnerMetrics : ISessionRunnerMetrics
{
    public static readonly NullSessionRunnerMetrics Instance = new();

    private NullSessionRunnerMetrics() { }

    public void OnBytesReceived(int bytes) { }
    public void OnMessageDispatched() { }
    public void OnReconnectAttempt(int attempt, TimeSpan delay) { }
    public void OnReconnectSuccess() { }
    public void OnReceiveFailure(Exception exception) { }
}
