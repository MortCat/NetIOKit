using System.Buffers;
using NetIOKit.Protocols;

namespace NetIOKit;

/// <summary>
/// Backward-compatible parser entry point.
/// Internally delegates to <see cref="LengthPrefixedPacketParser"/>.
/// </summary>
public sealed class PacketParser
{
    private readonly LengthPrefixedPacketParser _inner;

    public PacketParser(int maxPayloadLength = 1024 * 1024)
    {
        _inner = new LengthPrefixedPacketParser(maxPayloadLength);
    }

    public bool TryDecode(ReadOnlySequence<byte> source, out byte[] message, out int consumedBytes)
        => _inner.TryDecode(source, out message, out consumedBytes);

    public byte[] Encode(byte[] message)
        => _inner.Encode(message);
}
