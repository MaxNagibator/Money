namespace Money.Web.Common;

public class MoneyException : Exception
{
    public MoneyException()
    {
    }

    public MoneyException(string message) : base(message)
    {
    }

    public MoneyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
