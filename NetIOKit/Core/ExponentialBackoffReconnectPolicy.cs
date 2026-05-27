using NetIOKit.Abstractions;

namespace NetIOKit.Core;

public sealed class ExponentialBackoffReconnectPolicy : IReconnectPolicy
{
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    private readonly int _maxAttempts;
    private readonly double _jitterRatio;
    private readonly Random _random;

    public ExponentialBackoffReconnectPolicy(
        int maxAttempts = 5,
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null,
        double jitterRatio = 0.2,
        Random? random = null)
    {
        if (maxAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        if (jitterRatio < 0 || jitterRatio > 1) throw new ArgumentOutOfRangeException(nameof(jitterRatio));

        _maxAttempts = maxAttempts;
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(100);
        _maxDelay = maxDelay ?? TimeSpan.FromSeconds(5);
        _jitterRatio = jitterRatio;
        _random = random ?? new Random();
    }

    public bool ShouldRetry(int attempt, Exception exception)
    {
        return attempt <= _maxAttempts && exception is NetIOException;
    }

    public TimeSpan GetDelay(int attempt)
    {
        var exp = Math.Pow(2, Math.Max(0, attempt - 1));
        var baseMs = Math.Min(_baseDelay.TotalMilliseconds * exp, _maxDelay.TotalMilliseconds);
        var jitterRange = baseMs * _jitterRatio;
        var jitter = (_random.NextDouble() * 2 - 1) * jitterRange;
        var total = Math.Max(0, baseMs + jitter);
        return TimeSpan.FromMilliseconds(total);
    }
}
