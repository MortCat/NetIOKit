# NetIOKit Error Codes (v0)

| Code | Category | Meaning |
|---|---|---|
| `NIO-TRN-001` | Transport | Connection failed or unexpectedly broken |
| `NIO-SES-001` | Session | Session handshake or heartbeat timeout |
| `NIO-PRT-001` | Protocol | Invalid frame length or malformed packet |
| `NIO-STG-001` | Strategy | Queue overflow or strategy limit reached |

## Notes
- Use stable codes for cross-language parity and observability.
- Wrap internal exceptions with `NetIOException` to preserve machine-readable code.
