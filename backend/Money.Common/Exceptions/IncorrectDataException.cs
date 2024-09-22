namespace Money.Common.Exceptions;

public class IncorrectDataException(string message) : BusinessException(message);
