# NetIOKit Test Matrix (Phase 1 Draft)

## Functional
- Encode/decode round-trip for length-prefixed frames.
- Partial header buffering behavior.
- Partial payload buffering behavior.
- Multiple frames in single receive chunk.
- Invalid length rejection.

## Reliability / Fault Injection
- Abrupt transport disconnect during frame transfer.
- Slow stream with fragmented packets.
- Reconnect attempts with backoff configuration.
- Session disconnect idempotency.

## Performance
- Throughput under sustained burst load.
- P50/P95/P99 end-to-end latency.
- Memory pressure and allocation profile under high frame rate.

## Conformance (for multi-language)
- Golden packet corpus shared across C#/Python/C++/Java/Rust.
- State transition parity tests for session lifecycle.
- Common error code parity checks.
