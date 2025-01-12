namespace Money.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public NotFoundException(string entityType, int id, string additionalInfo = "Пожалуйста, проверьте правильность введенного ID") : base($"Извините, но {entityType} с ID {id} не найден. {additionalInfo}")
    {
    }
}
