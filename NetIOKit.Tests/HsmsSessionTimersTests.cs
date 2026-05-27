using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class HsmsSessionTimersTests
{
    [Fact]
    public void T7_Expires_WhenNotSelectedInTime()
    {
        var timers = new HsmsSessionTimers(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(50));
        var manager = new HsmsSessionLifecycleManager(new HsmsSessionStateMachine(), timers);

        var start = DateTimeOffset.UtcNow;
        manager.OnConnected(start);

        Assert.Throws<TimeoutException>(() => manager.ValidateNoTimeout(start.AddMilliseconds(80)));
    }

    [Fact]
    public void T6_Expires_WhenSelectRequestHasNoResponse()
    {
        var timers = new HsmsSessionTimers(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(200));
        var manager = new HsmsSessionLifecycleManager(new HsmsSessionStateMachine(), timers);

        var t0 = DateTimeOffset.UtcNow;
        manager.OnConnected(t0);
        manager.OnControlMessage(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100), t0);

        Assert.Throws<TimeoutException>(() => manager.ValidateNoTimeout(t0.AddMilliseconds(60)));
    }

    [Fact]
    public void T6_Stops_WhenResponseArrives()
    {
        var timers = new HsmsSessionTimers(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));
        var manager = new HsmsSessionLifecycleManager(new HsmsSessionStateMachine(), timers);

        var t0 = DateTimeOffset.UtcNow;
        manager.OnConnected(t0);
        manager.OnControlMessage(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100), t0);
        manager.OnControlMessage(new HsmsControlMessage(HsmsControlMessageType.SelectResponse, 1, 100, status: 0), t0.AddMilliseconds(20));

        manager.ValidateNoTimeout(t0.AddMilliseconds(150));
        Assert.Equal(HsmsConnectionState.Selected, manager.State);
    }
}
