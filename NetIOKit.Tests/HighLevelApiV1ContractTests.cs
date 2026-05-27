using System.Reflection;
using NetIOKit.Core;

namespace NetIOKit.Tests;

public sealed class HighLevelApiV1ContractTests
{
    [Fact]
    public void MessageFormat_Name_IsStable()
    {
        Assert.Equal("LengthPrefixedUtf8V1", NetIOMessageFormat.Name);
    }

    [Fact]
    public void ClientFacade_PublicApiShape_IsStable()
    {
        var type = typeof(NetIOClientFacade);

        Assert.NotNull(type.GetConstructor(new[]
        {
            typeof(string),
            typeof(int),
            typeof(Func<string, CancellationToken, ValueTask>)
        }));

        AssertPublicMethod(type, "ConnectAsync", typeof(ValueTask), typeof(CancellationToken));
        AssertPublicMethod(type, "RunAsync", typeof(Task), typeof(CancellationToken));
        AssertPublicMethod(type, "SendTextAsync", typeof(ValueTask), typeof(string), typeof(CancellationToken));
        AssertPublicMethod(type, "CloseAsync", typeof(ValueTask), typeof(CancellationToken));
        Assert.Contains(typeof(IAsyncDisposable), type.GetInterfaces());
    }

    [Fact]
    public void ServerFacade_PublicApiShape_IsStable()
    {
        var type = typeof(NetIOServerFacade);

        Assert.NotNull(type.GetConstructor(new[]
        {
            typeof(int),
            typeof(Func<string, CancellationToken, ValueTask<string>>)
        }));

        var portProperty = type.GetProperty("Port", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(portProperty);
        Assert.Equal(typeof(int), portProperty!.PropertyType);

        AssertPublicMethod(type, "Start", typeof(void));
        AssertPublicMethod(type, "RunOnceAsync", typeof(Task), typeof(CancellationToken));
        Assert.Contains(typeof(IAsyncDisposable), type.GetInterfaces());
    }

    [Fact]
    public async Task Facade_Contract_RoundTrip_PingAck_IsStable()
    {
        await using var server = new NetIOServerFacade(0, static (msg, _) => ValueTask.FromResult($"ACK:{msg}"));
        server.Start();

        var received = new List<string>();
        await using var client = new NetIOClientFacade("127.0.0.1", server.Port, (msg, _) =>
        {
            received.Add(msg);
            return ValueTask.CompletedTask;
        });

        await client.ConnectAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));
        var serverTask = server.RunOnceAsync(cts.Token);
        var clientTask = client.RunAsync(cts.Token);

        await client.SendTextAsync("PING", cts.Token);
        await Task.Delay(50, cts.Token);
        cts.Cancel();

        await Task.WhenAll(
            serverTask.ContinueWith(_ => { }),
            clientTask.ContinueWith(_ => { }));

        Assert.Contains("ACK:PING", received);
    }

    private static void AssertPublicMethod(Type type, string name, Type returnType, params Type[] parameterTypes)
    {
        var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, binder: null, types: parameterTypes, modifiers: null);
        Assert.NotNull(method);
        Assert.Equal(returnType, method!.ReturnType);
    }
}
