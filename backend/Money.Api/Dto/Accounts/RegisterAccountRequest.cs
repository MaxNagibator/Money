namespace Money.Api.Dto.Accounts;

/// <summary>
/// Запрос на регистрацию нового аккаунта.
/// </summary>
public class RegisterAccountRequest
{
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Адрес электронной почты пользователя.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Фабричный метод для создания бизнес-модели на основе DTO.
    /// </summary>
    /// <returns>Новый объект <see cref="RegisterAccount" />.</returns>
    public RegisterAccount ToBusinessModel()
    {
        return new()
        {
            UserName = UserName,
            Email = Email,
            Password = Password,
        };
    }
}
