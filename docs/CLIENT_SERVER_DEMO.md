# NetIOKit Minimal Client/Server Demo

## Purpose
Show the core-library-only path for a single request/response round trip:
- client sends one framed message
- server decodes and replies `ACK:<message>`
- client receives ACK through `SessionRunner + DefaultPipelineStrategy`

## Project
- `NetIOKit.Demo`

## Run
```bash
dotnet run --project NetIOKit.Demo -- "PING"
```

Expected output fields:
- `ServerReceived=PING`
- `ClientReceived=ACK:PING`

## Core API Used
- `TcpTransport`
- `SessionRunner<byte[]>`
- `LengthPrefixedPacketParser`
- `DefaultPipelineStrategy<byte[]>`
- `ProtocolReadBuffer<byte[]>` (server side framing)

No benchmark/report/export dependency is required.
