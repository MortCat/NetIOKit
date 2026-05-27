using NetIOKit.Abstractions;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Core;

/// <summary>
/// Minimal HSMS session wrapper that binds control-message dispatch to state machine updates.
/// Scope intentionally small: control path only (Select/Linktest).
/// </summary>
public sealed class NetIOHsmsSession
{
    private readonly SessionRunner<HsmsControlMessage> _runner;
    private readonly HsmsSessionStateMachine _stateMachine;

    public NetIOHsmsSession(ITransport transport, Action<HsmsControlMessage>? onMessage = null)
    {
        ArgumentNullException.ThrowIfNull(transport);

        _stateMachine = new HsmsSessionStateMachine();
        var codec = new HsmsControlMessageCodec();
        var strategy = new DefaultPipelineStrategy<HsmsControlMessage>((msg, _) =>
        {
            _stateMachine.Apply(msg);
            onMessage?.Invoke(msg);
            return ValueTask.CompletedTask;
        });

        _runner = new SessionRunner<HsmsControlMessage>(transport, codec, strategy);
    }

    public HsmsConnectionState State => _stateMachine.State;

    public Task RunAsync(CancellationToken cancellationToken = default) => _runner.RunAsync(cancellationToken);
}
