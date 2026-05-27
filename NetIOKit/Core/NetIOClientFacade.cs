using System.Text;
using NetIOKit.Protocols;
using NetIOKit.Strategies;

namespace NetIOKit.Core;

/// <summary>
/// 對外高階 Client Facade。
/// 封裝封包頭、長度、拆包/黏包與接收迴圈，使用者只需收發文字訊息。
/// </summary>
public sealed class NetIOClientFacade : IAsyncDisposable
{
    private readonly TcpTransport _transport;
    private readonly LengthPrefixedPacketParser _parser;
    private readonly SessionRunner<byte[]> _runner;

    public NetIOClientFacade(string host, int port, Func<string, CancellationToken, ValueTask> onMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(onMessage);

        _transport = new TcpTransport(new TcpEndpoint(host, port));
        _parser = new LengthPrefixedPacketParser();
        var strategy = new DefaultPipelineStrategy<byte[]>((payload, ct) =>
            onMessage(Encoding.UTF8.GetString(payload), ct));
        _runner = new SessionRunner<byte[]>(_transport, _parser, strategy);
    }

    public ValueTask ConnectAsync(CancellationToken cancellationToken = default) => _transport.OpenAsync(cancellationToken);

    public Task RunAsync(CancellationToken cancellationToken = default) => _runner.RunAsync(cancellationToken);

    public ValueTask SendTextAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        var frame = _parser.Encode(Encoding.UTF8.GetBytes(message));
        return _transport.SendAsync(frame, cancellationToken);
    }

    public ValueTask CloseAsync(CancellationToken cancellationToken = default) => _transport.CloseAsync(cancellationToken);

    public async ValueTask DisposeAsync() => await _transport.DisposeAsync().ConfigureAwait(false);
}
