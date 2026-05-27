# NetIOKit Protocols (v0)

## 1. Length-Prefixed Framing (Reference)

### 1.1 Frame Format
- Byte `[0..3]`: payload length (`Int32`, little-endian)
- Byte `[4..(4+N-1)]`: payload (`N` bytes)

### 1.2 Validation Rules
1. Length header must be present (at least 4 bytes buffered).
2. Payload length must be `0 <= N <= MaxPayloadLength`.
3. Full frame is considered complete only when buffered bytes >= `4 + N`.

### 1.3 Decoder Behavior
- If header is incomplete: return `false`, consume `0`.
- If payload is incomplete: return `false`, consume `0`.
- If frame is valid and complete: return `true`, output payload, consume full frame bytes.
- If payload length is invalid: throw protocol-level exception.

### 1.4 Stateful Buffering
`ProtocolReadBuffer<TMessage>` accumulates partial incoming bytes and repeatedly attempts decode.
When a frame succeeds, consumed bytes are removed and remaining bytes are retained for next decode.

## 2. Planned Protocol Adapters
- SECS/GEM adapter (client/server roles)
- Ethernet I/O adapter
- Vendor-specific TCP formats as independent codecs
