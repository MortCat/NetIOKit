using NetIOKit.Core;

namespace NetIOKit.Tests;

public sealed class MinimalClientServerDemoTests
{
    [Fact]
    public async Task RunAsync_RoundTripsMessageAndAck()
    {
        var result = await MinimalClientServerDemo.RunAsync("PING");

        Assert.Equal("PING", result.ServerReceivedMessage);
        Assert.Equal("ACK:PING", result.ClientReceivedAck);
    }

    [Fact]
    public async Task RunAsync_ThrowsForEmptyMessage()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => MinimalClientServerDemo.RunAsync(" "));
    }
}
