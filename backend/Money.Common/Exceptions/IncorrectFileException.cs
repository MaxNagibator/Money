namespace Money.Common.Exceptions;

public class IncorrectFileException : BusinessException
{
    public IncorrectFileException()
    {
    }

    public IncorrectFileException(string message) : base(message)
    {
    }

    public IncorrectFileException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
