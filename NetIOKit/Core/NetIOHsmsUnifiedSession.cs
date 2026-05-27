using NetIOKit.Abstractions;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Core;

/// <summary>
/// Minimal unified HSMS session that multiplexes control/data frames in one runner.
///
/// Behavior:
/// - Control frame: update HSMS state machine first, then invoke control callback.
/// - Data frame: invoke data callback.
///
/// This keeps control lifecycle deterministic while allowing data path extension.
/// </summary>
public sealed class NetIOHsmsUnifiedSession
{
    private readonly SessionRunner<HsmsUnifiedFrame> _runner;
    private readonly HsmsSessionStateMachine _stateMachine = new();

    public NetIOHsmsUnifiedSession(
        ITransport transport,
        Action<HsmsControlMessage>? onControl = null,
        Action<HsmsDataMessage>? onData = null)
    {
        ArgumentNullException.ThrowIfNull(transport);

        var strategy = new DefaultPipelineStrategy<HsmsUnifiedFrame>((frame, _) =>
        {
            DispatchFrame(frame, onControl, onData);
            return ValueTask.CompletedTask;
        });

        _runner = new SessionRunner<HsmsUnifiedFrame>(transport, new HsmsUnifiedCodec(), strategy);
    }

    public HsmsConnectionState State => _stateMachine.State;

    public Task RunAsync(CancellationToken cancellationToken = default) => _runner.RunAsync(cancellationToken);

    private void DispatchFrame(
        HsmsUnifiedFrame frame,
        Action<HsmsControlMessage>? onControl,
        Action<HsmsDataMessage>? onData)
    {
        if (frame.Kind == HsmsFrameKind.Control && frame.Control.HasValue)
        {
            _stateMachine.Apply(frame.Control.Value);
            onControl?.Invoke(frame.Control.Value);
            return;
        }

        if (frame.Kind == HsmsFrameKind.Data && frame.Data.HasValue)
        {
            onData?.Invoke(frame.Data.Value);
        }
    }
}
