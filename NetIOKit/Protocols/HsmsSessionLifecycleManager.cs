namespace NetIOKit.Protocols;

/// <summary>
/// Minimal lifecycle manager that couples HSMS state transitions with T6/T7 timer policy.
/// Scope is intentionally small for scaffold stage.
/// </summary>
public sealed class HsmsSessionLifecycleManager
{
    private readonly HsmsSessionStateMachine _machine;
    private readonly HsmsSessionTimers _timers;

    public HsmsSessionLifecycleManager(HsmsSessionStateMachine machine, HsmsSessionTimers timers)
    {
        _machine = machine ?? throw new ArgumentNullException(nameof(machine));
        _timers = timers ?? throw new ArgumentNullException(nameof(timers));
    }

    public HsmsConnectionState State => _machine.State;

    public void OnConnected(DateTimeOffset now)
    {
        _timers.StartT7(now);
    }

    public void OnControlMessage(HsmsControlMessage message, DateTimeOffset now)
    {
        _machine.Apply(message);

        if (message.Type is HsmsControlMessageType.SelectRequest or HsmsControlMessageType.SelectResponse)
        {
            _timers.StopT7();
        }

        if (message.Type is HsmsControlMessageType.SelectRequest or HsmsControlMessageType.LinktestRequest)
        {
            _timers.StartT6(now);
            return;
        }

        if (message.Type is HsmsControlMessageType.SelectResponse or HsmsControlMessageType.LinktestResponse)
        {
            _timers.StopT6();
        }
    }

    public void ValidateNoTimeout(DateTimeOffset now)
    {
        if (_timers.IsT7Expired(now))
        {
            throw new TimeoutException("HSMS T7 timeout: session not selected in time.");
        }

        if (_timers.IsT6Expired(now))
        {
            throw new TimeoutException("HSMS T6 timeout: control transaction response not received in time.");
        }
    }
}
