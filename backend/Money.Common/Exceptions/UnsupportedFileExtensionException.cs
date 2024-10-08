namespace Money.Common.Exceptions;

public class UnsupportedFileExtensionException(string message) : BusinessException(message);