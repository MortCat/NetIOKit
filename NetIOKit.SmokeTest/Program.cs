using NetIOKit.Core;

Console.WriteLine("NetIOKit Smoke Test (Simple/Direct)");
Console.WriteLine("1) Starts local server facade");
Console.WriteLine("2) Connects client facade");
Console.WriteLine("3) Sends PING and expects ACK:PING");

await using var server = new NetIOServerFacade(0, static (msg, _) => ValueTask.FromResult($"ACK:{msg}"));
server.Start();

string? ack = null;
await using var client = new NetIOClientFacade("127.0.0.1", server.Port, (msg, _) =>
{
    ack = msg;
    return ValueTask.CompletedTask;
});

await client.ConnectAsync();
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

var serverTask = server.RunOnceAsync(cts.Token);
var clientTask = client.RunAsync(cts.Token);

await client.SendTextAsync("PING", cts.Token);
await Task.Delay(100, cts.Token);
cts.Cancel();

await Task.WhenAll(
    serverTask.ContinueWith(_ => { }),
    clientTask.ContinueWith(_ => { }));

Console.WriteLine($"Received: {ack}");
Console.WriteLine(ack == "ACK:PING" ? "PASS" : "FAIL");
