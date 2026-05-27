using NetIOKit.Abstractions;

namespace NetIOKit.Core;

public abstract class SessionBase : ISession
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    protected SessionBase(string? sessionId = null)
    {
        SessionId = sessionId ?? Guid.NewGuid().ToString("N");
    }

    public string SessionId { get; }
    public bool IsConnected { get; protected set; }

    public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            if (IsConnected)
            {
                return;
            }

            await OnConnectAsync(cancellationToken).ConfigureAwait(false);
            IsConnected = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsConnected)
            {
                return;
            }

            await OnDisconnectAsync(cancellationToken).ConfigureAwait(false);
            IsConnected = false;
        }
        finally
        {
            _gate.Release();
        }
    }

    protected abstract ValueTask OnConnectAsync(CancellationToken cancellationToken);
    protected abstract ValueTask OnDisconnectAsync(CancellationToken cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);
        _gate.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
