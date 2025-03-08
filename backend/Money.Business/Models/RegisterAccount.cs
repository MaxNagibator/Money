namespace Money.Business.Models;

/// <summary>
/// Новый аккаунт.
/// </summary>
public class RegisterAccount
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
}
