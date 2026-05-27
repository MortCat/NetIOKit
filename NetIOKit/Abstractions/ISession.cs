namespace NetIOKit.Abstractions;

public interface ISession : IAsyncDisposable
{
    string SessionId { get; }
    bool IsConnected { get; }

    ValueTask ConnectAsync(CancellationToken cancellationToken = default);
    ValueTask DisconnectAsync(CancellationToken cancellationToken = default);
}
