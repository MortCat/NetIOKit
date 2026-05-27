using NetIOKit.Core;

namespace NetIOKit.Tests;

public class SessionBaseTests
{
    [Fact]
    public async Task ConnectAsync_IsIdempotent_WhenCalledConcurrently()
    {
        var session = new FakeSession(connectDelayMs: 20);

        var tasks = Enumerable.Range(0, 16)
            .Select(_ => session.ConnectAsync().AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.True(session.IsConnected);
        Assert.Equal(1, session.ConnectCallCount);
    }

    [Fact]
    public async Task DisconnectAsync_IsIdempotent_WhenCalledConcurrently()
    {
        var session = new FakeSession();
        await session.ConnectAsync();

        var tasks = Enumerable.Range(0, 16)
            .Select(_ => session.DisconnectAsync().AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.False(session.IsConnected);
        Assert.Equal(1, session.DisconnectCallCount);
    }

    [Fact]
    public async Task DisposeAsync_CallsDisconnectOnce()
    {
        var session = new FakeSession();
        await session.ConnectAsync();

        await session.DisposeAsync();
        await session.DisposeAsync();

        Assert.False(session.IsConnected);
        Assert.Equal(1, session.DisconnectCallCount);
    }

    private sealed class FakeSession : SessionBase
    {
        private readonly int _connectDelayMs;

        public FakeSession(int connectDelayMs = 0)
        {
            _connectDelayMs = connectDelayMs;
        }

        public int ConnectCallCount { get; private set; }
        public int DisconnectCallCount { get; private set; }

        protected override async ValueTask OnConnectAsync(CancellationToken cancellationToken)
        {
            ConnectCallCount++;
            if (_connectDelayMs > 0)
            {
                await Task.Delay(_connectDelayMs, cancellationToken);
            }
        }

        protected override ValueTask OnDisconnectAsync(CancellationToken cancellationToken)
        {
            DisconnectCallCount++;
            return ValueTask.CompletedTask;
        }
    }
}
