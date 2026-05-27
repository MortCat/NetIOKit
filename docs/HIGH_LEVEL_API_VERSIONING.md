# High-Level API v1 Versioning & Compatibility Policy

## Scope
This policy covers the public high-level API surface:
- `NetIOClientFacade`
- `NetIOServerFacade`
- `NetIOMessageFormat.Name`

## Compatibility Contract (v1)
The following are treated as **stable contract** in v1:

1. `NetIOMessageFormat.Name == "LengthPrefixedUtf8V1"`.
2. `NetIOClientFacade` constructor signature:
   - `(string host, int port, Func<string, CancellationToken, ValueTask> onMessage)`
3. `NetIOClientFacade` public methods:
   - `ConnectAsync(CancellationToken)`
   - `RunAsync(CancellationToken)`
   - `SendTextAsync(string, CancellationToken)`
   - `CloseAsync(CancellationToken)`
4. `NetIOServerFacade` constructor signature:
   - `(int port, Func<string, CancellationToken, ValueTask<string>> onMessage)`
5. `NetIOServerFacade` public members:
   - `Port` property
   - `Start()`
   - `RunOnceAsync(CancellationToken)`
6. Round-trip behavior baseline:
   - request `PING` can yield response `ACK:PING` when server handler returns that pattern.

## Non-Breaking Changes (Allowed in v1)
- Additive APIs (new optional overloads, new helper types).
- Internal refactors that preserve the above contract.
- Performance improvements that do not alter contract behavior.

## Breaking Changes (Require v2)
- Renaming/removing public members listed above.
- Changing signatures or return types of listed members.
- Changing default message format semantic from `LengthPrefixedUtf8V1`.

## Enforcement
- Contract tests in `NetIOKit.Tests/HighLevelApiV1ContractTests.cs` act as CI guardrails.
- Any contract-breaking update must:
  1) bump major API version doc section,
  2) introduce migration notes,
  3) update contract tests explicitly.
