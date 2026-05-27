using NetIOKit.Abstractions;

namespace NetIOKit.Strategies;

public sealed class DefaultPipelineStrategy<TMessage> : IPipelineStrategy<TMessage>
{
    private readonly Func<TMessage, CancellationToken, ValueTask> _handler;

    public DefaultPipelineStrategy(Func<TMessage, CancellationToken, ValueTask> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public ValueTask HandleAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        return _handler(message, cancellationToken);
    }
}
