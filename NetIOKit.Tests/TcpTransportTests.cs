using System.Net;
using System.Net.Sockets;
using NetIOKit.Core;

namespace NetIOKit.Tests;

public class TcpTransportTests
{
    [Fact]
    public async Task TcpTransport_CanConnectSendReceiveLoopback()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            await using var stream = client.GetStream();

            var buffer = new byte[16];
            var read = await stream.ReadAsync(buffer);
            await stream.WriteAsync(buffer.AsMemory(0, read));
        });

        await using var transport = new TcpTransport(new TcpEndpoint("127.0.0.1", port));
        await transport.OpenAsync();

        var payload = new byte[] { 9, 8, 7 };
        await transport.SendAsync(payload);

        var recv = new byte[16];
        var n = await transport.ReceiveAsync(recv);

        Assert.Equal(payload.Length, n);
        Assert.Equal(payload, recv.AsSpan(0, n).ToArray());

        await transport.CloseAsync();
        listener.Stop();
        await serverTask;
    }
}
