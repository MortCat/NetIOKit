namespace NetIOKit.Abstractions;

public interface IPipelineStrategy<TMessage>
{
    ValueTask HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}
