using NetIOKit.Abstractions;

namespace NetIOKit.Core;

public sealed record TcpEndpoint(string Host, int Port) : ITcpEndpoint;
