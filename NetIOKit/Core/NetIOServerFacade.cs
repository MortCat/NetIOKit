using System.Net;
using System.Net.Sockets;
using System.Text;
using NetIOKit.Protocols;

namespace NetIOKit.Core;

/// <summary>
/// 對外高階 Server Facade。
/// 內部處理 framing/解包，對外提供純文字 request/response 委派。
/// </summary>
public sealed class NetIOServerFacade : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly Func<string, CancellationToken, ValueTask<string>> _onMessage;
    private readonly LengthPrefixedPacketParser _parser = new();

    public NetIOServerFacade(int port, Func<string, CancellationToken, ValueTask<string>> onMessage)
    {
        ArgumentNullException.ThrowIfNull(onMessage);
        _onMessage = onMessage;
        _listener = new TcpListener(IPAddress.Loopback, port);
    }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public void Start() => _listener.Start();

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        using var socket = await _listener.AcceptSocketAsync(cancellationToken).ConfigureAwait(false);
        await using var stream = new NetworkStream(socket, ownsSocket: false);
        var readBuffer = new ProtocolReadBuffer<byte[]>(_parser);
        var buffer = new byte[4096];

        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read <= 0) return;

            readBuffer.Append(buffer.AsSpan(0, read));
            while (readBuffer.TryRead(out var payload))
            {
                var request = Encoding.UTF8.GetString(payload);
                var responseText = await _onMessage(request, cancellationToken).ConfigureAwait(false);
                var responseFrame = _parser.Encode(Encoding.UTF8.GetBytes(responseText));
                await stream.WriteAsync(responseFrame, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _listener.Stop();
        return ValueTask.CompletedTask;
    }
}
