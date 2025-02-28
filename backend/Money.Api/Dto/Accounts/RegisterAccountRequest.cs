namespace Money.Api.Dto.Accounts;

public class RegisterAccountRequest
{
    public required string UserName { get; set; }
    public string? Email { get; set; }
    public required string Password { get; set; }

    /// <summary>
    /// Фабричный метод для создания бизнес-модели на основе DTO.
    /// </summary>
    /// <returns>Новый объект <see cref="CarEvent" />.</returns>
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
