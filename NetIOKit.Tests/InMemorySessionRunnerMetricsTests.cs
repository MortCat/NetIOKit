using NetIOKit.Core;

namespace NetIOKit.Tests;

public sealed class InMemorySessionRunnerMetricsTests
{
    [Fact]
    public void Snapshot_ReturnsCurrentCounters()
    {
        var metrics = new InMemorySessionRunnerMetrics();

        metrics.OnBytesReceived(128);
        metrics.OnMessageDispatched();
        metrics.OnReconnectAttempt(1, TimeSpan.FromMilliseconds(100));
        metrics.OnReconnectSuccess();
        metrics.OnReceiveFailure(new InvalidOperationException("boom"));

        var snapshot = metrics.GetSnapshot();

        Assert.Equal(128, snapshot.BytesReceived);
        Assert.Equal(1, snapshot.MessagesDispatched);
        Assert.Equal(1, snapshot.ReconnectAttempts);
        Assert.Equal(1, snapshot.ReconnectSuccess);
        Assert.Equal(1, snapshot.ReceiveFailures);
    }

    [Fact]
    public void Reset_ClearsAllCounters()
    {
        var metrics = new InMemorySessionRunnerMetrics();

        metrics.OnBytesReceived(128);
        metrics.OnMessageDispatched();
        metrics.OnReconnectAttempt(1, TimeSpan.FromMilliseconds(100));
        metrics.OnReconnectSuccess();
        metrics.OnReceiveFailure(new InvalidOperationException("boom"));

        metrics.Reset();

        var snapshot = metrics.GetSnapshot();
        Assert.Equal(0, snapshot.BytesReceived);
        Assert.Equal(0, snapshot.MessagesDispatched);
        Assert.Equal(0, snapshot.ReconnectAttempts);
        Assert.Equal(0, snapshot.ReconnectSuccess);
        Assert.Equal(0, snapshot.ReceiveFailures);
    }

    [Fact]
    public void GetMessagesPerSecond_ReturnsNonNegativeValue()
    {
        var metrics = new InMemorySessionRunnerMetrics();
        metrics.OnMessageDispatched();

        var value = metrics.GetMessagesPerSecond();

        Assert.True(value >= 0);
    }
}
