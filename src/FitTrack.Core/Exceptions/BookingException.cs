namespace FitTrack.Core.Exceptions;

public class BookingException : Exception
{
    public ErrorKind Kind { get; }
    public BookingException(string message, ErrorKind kind) : base(message)
    {
        Kind = kind;
    }
}