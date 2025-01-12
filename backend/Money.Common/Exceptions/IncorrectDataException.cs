namespace Money.Common.Exceptions;

public class IncorrectDataException : BusinessException
{
    public IncorrectDataException()
    {
    }

    public IncorrectDataException(string message) : base(message)
    {
    }

    public IncorrectDataException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
