namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS timer scaffold (T6/T7) with timeout tracking hooks.
/// T6: control transaction timeout (e.g., Select/Linktest response wait)
/// T7: not-selected timeout (connection must be selected within deadline)
/// </summary>
public sealed class HsmsSessionTimers
{
    private readonly TimeSpan _t6;
    private readonly TimeSpan _t7;
    private DateTimeOffset? _t6Deadline;
    private DateTimeOffset? _t7Deadline;

    public HsmsSessionTimers(TimeSpan t6, TimeSpan t7)
    {
        if (t6 <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(t6));
        if (t7 <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(t7));
        _t6 = t6;
        _t7 = t7;
    }

    public bool IsT6Running => _t6Deadline.HasValue;
    public bool IsT7Running => _t7Deadline.HasValue;

    public void StartT6(DateTimeOffset now) => _t6Deadline = now + _t6;
    public void StopT6() => _t6Deadline = null;

    public void StartT7(DateTimeOffset now) => _t7Deadline = now + _t7;
    public void StopT7() => _t7Deadline = null;

    public bool IsT6Expired(DateTimeOffset now) => _t6Deadline.HasValue && now >= _t6Deadline.Value;
    public bool IsT7Expired(DateTimeOffset now) => _t7Deadline.HasValue && now >= _t7Deadline.Value;
}
