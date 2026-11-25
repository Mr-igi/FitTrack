namespace FitTrack.Core.Exceptions;

public class MembershipException : Exception
{
    public ErrorKind Kind { get; }

    public MembershipException(string message, ErrorKind kind) : base(message)
    {
        Kind = kind;
    }
}