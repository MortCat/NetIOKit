using System.Net.Sockets;
using NetIOKit.Abstractions;
using NetIOKit.Protocols;

namespace NetIOKit.Core;

public sealed class SessionRunner<TMessage>
{
    private readonly ITransport _transport;
    private readonly ProtocolReadBuffer<TMessage> _readBuffer;
    private readonly IPipelineStrategy<TMessage> _strategy;
    private readonly int _receiveBufferSize;
    private readonly IReconnectPolicy? _reconnectPolicy;
    private readonly ISessionRunnerMetrics _metrics;

    public SessionRunner(
        ITransport transport,
        IProtocolCodec<TMessage> codec,
        IPipelineStrategy<TMessage> strategy,
        int receiveBufferSize = 4096,
        IReconnectPolicy? reconnectPolicy = null,
        ISessionRunnerMetrics? metrics = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _readBuffer = new ProtocolReadBuffer<TMessage>(codec ?? throw new ArgumentNullException(nameof(codec)));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _reconnectPolicy = reconnectPolicy;
        _metrics = metrics ?? NullSessionRunnerMetrics.Instance;

        if (receiveBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(receiveBufferSize));
        }

        _receiveBufferSize = receiveBufferSize;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var attempt = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunReceiveLoopAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                attempt++;

                if (_reconnectPolicy is null || !_reconnectPolicy.ShouldRetry(attempt, ex))
                {
                    throw;
                }

                var delay = _reconnectPolicy.GetDelay(attempt);
                _metrics.OnReconnectAttempt(attempt, delay);
                await _transport.CloseAsync(cancellationToken).ConfigureAwait(false);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                await _transport.OpenAsync(cancellationToken).ConfigureAwait(false);
                _metrics.OnReconnectSuccess();
            }
        }
    }

    private async Task RunReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var receiveBuffer = new byte[_receiveBufferSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            int read;
            try
            {
                read = await _transport.ReceiveAsync(receiveBuffer, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException or SocketException)
            {
                _metrics.OnReceiveFailure(ex);
                throw new NetIOException(ErrorCodes.TransportConnectionFailed, "Receive failed in session loop.", ex);
            }

            if (read <= 0)
            {
                throw new NetIOException(ErrorCodes.TransportConnectionFailed, "Connection closed by remote endpoint.");
            }

            _metrics.OnBytesReceived(read);
            _readBuffer.Append(receiveBuffer.AsSpan(0, read));

            while (_readBuffer.TryRead(out var message))
            {
                await _strategy.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                _metrics.OnMessageDispatched();
            }
        }
    }
}
