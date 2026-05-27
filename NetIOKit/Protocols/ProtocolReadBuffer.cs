using NetIOKit.Abstractions;
using System.Buffers;

namespace NetIOKit.Protocols;

/// <summary>
/// Accumulates incoming bytes and extracts messages using a codec.
/// </summary>
public sealed class ProtocolReadBuffer<TMessage>
{
    private readonly IProtocolCodec<TMessage> _codec;
    private readonly ArrayBufferWriter<byte> _buffer = new();

    public ProtocolReadBuffer(IProtocolCodec<TMessage> codec)
    {
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
    }

    public void Append(ReadOnlySpan<byte> bytes)
    {
        var target = _buffer.GetSpan(bytes.Length);
        bytes.CopyTo(target);
        _buffer.Advance(bytes.Length);
    }

    public bool TryRead(out TMessage message)
    {
        var sequence = new ReadOnlySequence<byte>(_buffer.WrittenMemory);

        if (!_codec.TryDecode(sequence, out message!, out var consumed))
        {
            return false;
        }

        if (consumed <= 0)
        {
            throw new InvalidOperationException("Codec consumed zero bytes while returning success.");
        }

        var remaining = _buffer.WrittenSpan[consumed..].ToArray();
        _buffer.Clear();
        Append(remaining);
        return true;
    }

    public int BufferedLength => _buffer.WrittenCount;
}
