using NetIOKit.Protocols;

namespace NetIOKit.Tests;

public sealed class HsmsSessionStateMachineTests
{
    [Fact]
    public void Apply_SelectRequest_MovesToSelected()
    {
        var machine = new HsmsSessionStateMachine();

        machine.Apply(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100));

        Assert.Equal(HsmsConnectionState.Selected, machine.State);
    }

    [Fact]
    public void Apply_LinktestInSelected_KeepsSelected()
    {
        var machine = new HsmsSessionStateMachine();
        machine.Apply(new HsmsControlMessage(HsmsControlMessageType.SelectRequest, 1, 100));

        machine.Apply(new HsmsControlMessage(HsmsControlMessageType.LinktestRequest, 1, 101));

        Assert.Equal(HsmsConnectionState.Selected, machine.State);
    }

    [Fact]
    public void Apply_LinktestBeforeSelect_Throws()
    {
        var machine = new HsmsSessionStateMachine();

        Assert.Throws<InvalidOperationException>(() =>
            machine.Apply(new HsmsControlMessage(HsmsControlMessageType.LinktestRequest, 1, 101)));
    }
}
