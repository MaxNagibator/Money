namespace Money.Common.Exceptions;

public class UnsupportedFileExtensionException : BusinessException
{
    public UnsupportedFileExtensionException()
    {
    }

    public UnsupportedFileExtensionException(string message) : base(message)
    {
    }

    public UnsupportedFileExtensionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
