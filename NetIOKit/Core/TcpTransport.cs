using System.Net.Sockets;
using NetIOKit.Abstractions;

namespace NetIOKit.Core;

public sealed class TcpTransport : ITransport
{
    private readonly ITcpEndpoint _endpoint;
    private TcpClient? _client;
    private NetworkStream? _stream;

    public TcpTransport(ITcpEndpoint endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    public async ValueTask OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_client is { Connected: true })
        {
            return;
        }

        _client = new TcpClient();
        using var reg = cancellationToken.Register(() => _client.Dispose());
        await _client.ConnectAsync(_endpoint.Host, _endpoint.Port, cancellationToken).ConfigureAwait(false);
        _stream = _client.GetStream();
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        await _stream!.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        return await _stream!.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        _stream?.Close();
        _client?.Close();
        _stream = null;
        _client = null;
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
    }

    private void EnsureOpen()
    {
        if (_stream is null || _client is null || !_client.Connected)
        {
            throw new NetIOException(ErrorCodes.TransportConnectionFailed, "TCP transport is not connected.");
        }
    }
}
