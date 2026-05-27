namespace NetIOKit.Abstractions;

public interface ISessionRunnerMetrics
{
    void OnBytesReceived(int bytes);
    void OnMessageDispatched();
    void OnReconnectAttempt(int attempt, TimeSpan delay);
    void OnReconnectSuccess();
    void OnReceiveFailure(Exception exception);
}
