namespace NetIOKit.Abstractions;

public interface IReconnectPolicy
{
    bool ShouldRetry(int attempt, Exception exception);
    TimeSpan GetDelay(int attempt);
}
