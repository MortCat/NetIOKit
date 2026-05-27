namespace NetIOKit.Protocols;

/// <summary>
/// Minimal HSMS control lifecycle state machine scaffold.
/// Supported transitions:
/// - NotSelected + SelectRequest/SelectResponse(success) => Selected
/// - Selected + LinktestRequest/LinktestResponse => keep Selected
/// Any unsupported control message in current state throws.
/// </summary>
public sealed class HsmsSessionStateMachine
{
    public HsmsConnectionState State { get; private set; } = HsmsConnectionState.NotSelected;

    public void Apply(HsmsControlMessage message)
    {
        switch (State)
        {
            case HsmsConnectionState.NotSelected:
                ApplyFromNotSelected(message);
                break;
            case HsmsConnectionState.Selected:
                ApplyFromSelected(message);
                break;
            default:
                throw new InvalidOperationException($"Unknown HSMS state: {State}");
        }
    }

    private void ApplyFromNotSelected(HsmsControlMessage message)
    {
        if (message.Type == HsmsControlMessageType.SelectRequest)
        {
            State = HsmsConnectionState.Selected;
            return;
        }

        if (message.Type == HsmsControlMessageType.SelectResponse && message.Status == 0)
        {
            State = HsmsConnectionState.Selected;
            return;
        }

        throw new InvalidOperationException($"Unsupported control message {message.Type} in state {State}");
    }

    private void ApplyFromSelected(HsmsControlMessage message)
    {
        if (message.Type is HsmsControlMessageType.LinktestRequest or HsmsControlMessageType.LinktestResponse)
        {
            return;
        }

        if (message.Type == HsmsControlMessageType.SelectRequest || message.Type == HsmsControlMessageType.SelectResponse)
        {
            return;
        }

        throw new InvalidOperationException($"Unsupported control message {message.Type} in state {State}");
    }
}
