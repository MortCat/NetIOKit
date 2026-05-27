using NetIOKit.Core;

namespace NetIOKit.Tests;

public sealed class ClientServerFacadeIntegrationTests
{
    [Fact]
    public async Task Facade_ClientServer_RoundTripText()
    {
        await using var server = new NetIOServerFacade(0, (msg, _) => ValueTask.FromResult($"ACK:{msg}"));
        server.Start();

        var received = new List<string>();
        await using var client = new NetIOClientFacade("127.0.0.1", server.Port, (msg, _) =>
        {
            received.Add(msg);
            return ValueTask.CompletedTask;
        });

        await client.ConnectAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var serverTask = server.RunOnceAsync(cts.Token);
        var clientTask = client.RunAsync(cts.Token);

        await client.SendTextAsync("PING", cts.Token);
        await Task.Delay(30, cts.Token);
        cts.Cancel();

        await Task.WhenAll(
            serverTask.ContinueWith(_ => { }),
            clientTask.ContinueWith(_ => { }));

        Assert.Contains("ACK:PING", received);
    }
}
