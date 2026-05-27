using System.Buffers;

namespace NetIOKit.Abstractions;

public interface IProtocolCodec<TMessage>
{
    bool TryDecode(ReadOnlySequence<byte> source, out TMessage message, out int consumedBytes);
    byte[] Encode(TMessage message);
}
