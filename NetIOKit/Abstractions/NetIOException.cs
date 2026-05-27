namespace NetIOKit.Abstractions;

public sealed class NetIOException : Exception
{
    public NetIOException(string code, string message, Exception? innerException = null)
        : base($"[{code}] {message}", innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
