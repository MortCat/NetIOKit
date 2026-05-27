# NetIOKit Benchmark/Soak Harness v1

## Purpose
First runnable harness for throughput + latency percentiles on loopback TCP.

## Project
- `NetIOKit.Benchmarks`

## Run
```bash
dotnet run --project NetIOKit.Benchmarks -- mode=throughput duration=15 payload=256 clients=4
```

```bash
dotnet run --project NetIOKit.Benchmarks -- mode=soak duration=3600 payload=256 clients=8
```

## Output Metrics
- `TotalMessages`
- `TotalBytes`
- `MessagesPerSecond`
- `MegabytesPerSecond`
- `LatencyP50Ms`
- `LatencyP95Ms`
- `LatencyP99Ms`
- `Duration`

## Notes
- v1 uses loopback echo (`127.0.0.1`) to reduce external network noise.
- `mode=soak` currently reuses the same run loop and is mainly a long-duration profile entry point.
- Next revision will add JSON/CSV export and reconnect churn scenarios.
