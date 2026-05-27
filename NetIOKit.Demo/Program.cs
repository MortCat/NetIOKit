using NetIOKit.Core;

var input = args.Length > 0 ? string.Join(' ', args) : "HELLO_NETIOKIT";
var result = await MinimalClientServerDemo.RunAsync(input);

Console.WriteLine("NetIOKit Minimal Client/Server Demo");
Console.WriteLine($"ServerReceived={result.ServerReceivedMessage}");
Console.WriteLine($"ClientReceived={result.ClientReceivedAck}");
