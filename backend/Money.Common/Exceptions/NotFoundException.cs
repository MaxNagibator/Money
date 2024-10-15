namespace Money.Common.Exceptions;

public class NotFoundException(string entityType, int id, string additionalInfo = "Пожалуйста, проверьте правильность введенного ID")
    : Exception($"Извините, но {entityType} с ID {id} не найден. {additionalInfo}");
