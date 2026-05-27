using System.Collections.Concurrent;
using NetIOKit.Abstractions;

namespace NetIOKit.Tests;

internal sealed class FakeTransport : ITransport
{
    private readonly ConcurrentQueue<byte[]> _segments = new();
    private readonly SemaphoreSlim _signal = new(0);
    private volatile bool _opened;
    private volatile bool _closed;

    public bool ThrowOnNextReceive { get; set; }

    public ValueTask OpenAsync(CancellationToken cancellationToken = default)
    {
        _opened = true;
        _closed = false;
        return ValueTask.CompletedTask;
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (!_opened || _closed)
        {
            throw new InvalidOperationException("Transport is not open.");
        }

        EnqueueIncoming(payload.ToArray());
        return ValueTask.CompletedTask;
    }

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (!_opened || _closed)
        {
            return 0;
        }

        await _signal.WaitAsync(cancellationToken);

        if (ThrowOnNextReceive)
        {
            ThrowOnNextReceive = false;
            throw new IOException("Simulated disconnect");
        }

        if (!_segments.TryDequeue(out var segment))
        {
            return 0;
        }

        segment.AsSpan().CopyTo(buffer.Span);
        return segment.Length;
    }

    public ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        _closed = true;
        return ValueTask.CompletedTask;
    }

    public void EnqueueIncoming(params byte[][] segments)
    {
        foreach (var seg in segments)
        {
            _segments.Enqueue(seg);
            _signal.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        _signal.Dispose();
    }
}
