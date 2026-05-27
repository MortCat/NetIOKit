namespace NetIOKit.Abstractions;

public interface ITcpEndpoint
{
    string Host { get; }
    int Port { get; }
}
