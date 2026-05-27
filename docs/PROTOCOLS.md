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


## 3. SECS/GEM-Style Scaffold (v0 Contract)

### 3.1 Skeleton Wire Format
- Byte `[0..3]`: `TotalLength` (`Int32`, little-endian), where `TotalLength = 3 + BodyLength`
- Byte `[4]`: `Stream (S)`
- Byte `[5]`: `Function (F)`
- Byte `[6]`: `Flags` (bit0 = `WaitBit`)
- Byte `[7..N-1]`: UTF-8 body placeholder

### 3.2 Scope Note
- This is **not** full HSMS/SECS-II yet.
- It is a compatibility scaffold so API/codec contracts can be locked before full protocol expansion.


## 4. SECS-II Item Model (Minimal Subset)

### 4.1 Supported Item Types
- `ASCII` (format code `0x41`)
- `U4` (format code `0xB1`, UInt32)

### 4.2 Item Wire Format (Scaffold)
- Byte `[0]`: `FormatCode`
- Byte `[1..4]`: `Length` (`Int32`, little-endian)
- Byte `[5..N-1]`: payload bytes

### 4.3 Scope Note
- This is a minimal item subset for compatibility/conformance tests.
- Full SECS-II item tree and HSMS session semantics are planned next.


## 5. HSMS Control Message Subset (Scaffold)

### 5.1 Supported Types
- `SelectRequest`
- `SelectResponse`
- `LinktestRequest`
- `LinktestResponse`

### 5.2 Wire Format
- Byte `[0..3]`: `TotalLength` (`Int32`, little-endian), fixed `8`
- Byte `[4]`: `MessageType`
- Byte `[5..6]`: `SessionId` (`UInt16`, little-endian)
- Byte `[7..10]`: `SystemBytes` (`UInt32`, little-endian)
- Byte `[11]`: `Status`

### 5.3 Scope Note
- This is a minimal control-message scaffold only.
- Full HSMS state machine/timers/selection lifecycle will be expanded next.


### 5.4 HSMS Minimal State Machine (Scaffold)
- Initial state: `NotSelected`
- `SelectRequest` or successful `SelectResponse(status=0)` => `Selected`
- In `Selected`, `LinktestRequest/LinktestResponse` are accepted and state remains `Selected`.
- Unsupported message/state combinations throw lifecycle exception.


### 5.5 Session Pipeline Binding (Current)
- `NetIOHsmsSession` binds `SessionRunner<HsmsControlMessage>` to `HsmsSessionStateMachine`.
- Each decoded control message is applied to state machine before optional callback dispatch.
- Current scope: control path only (no full HSMS data-message channel yet).


## 6. HSMS Data Message Scaffold (Control/Data Coexistence Step)

### 6.1 Wire Format
- Byte `[0..3]`: `BodyLength` (`Int32`, little-endian), `BodyLength = 9 + PayloadLength`
- Byte `[4..5]`: `SessionId` (`UInt16`, little-endian)
- Byte `[6..9]`: `SystemBytes` (`UInt32`, little-endian)
- Byte `[10]`: `Stream`
- Byte `[11]`: `Function`
- Byte `[12]`: `Flags` (bit0 = WaitBit)
- Byte `[13..N-1]`: payload bytes

### 6.2 Scope Note
- Current stage validates data path codec + coexistence test with control path on separate transports.
- Unified HSMS channel multiplexing remains a next step.


## 7. HSMS Unified Multiplexing Skeleton

### 7.1 Unified Frame
- `HsmsUnifiedFrame` wraps either Control or Data frame with `HsmsFrameKind`.

### 7.2 Unified Wire Envelope
- Byte `[0]`: `FrameKind` (`0=Control`, `1=Data`)
- Byte `[1..N-1]`: inner encoded frame bytes (control codec or data codec)

### 7.3 Current Scope
- Control frames update HSMS state machine.
- Data frames are dispatched as payload events.
- Full production multiplex policy/timers/retry windows are next-step work.


## 8. HSMS Session Timers (T6/T7 Scaffold)

### 8.1 Timer Intent
- `T6`: control transaction timeout (request -> response window)
- `T7`: selection timeout after connection established

### 8.2 Current Scaffold Policy
- On connection: start `T7`
- On `SelectRequest`: start `T6`
- On successful `SelectResponse`: stop `T6`, stop `T7`
- On `LinktestRequest`: start `T6`
- On `LinktestResponse`: stop `T6`

### 8.3 Scope Note
- This stage provides timeout tracking and validation hooks.
- Integration with full async scheduler/retry/reconnect policy is next-step work.
