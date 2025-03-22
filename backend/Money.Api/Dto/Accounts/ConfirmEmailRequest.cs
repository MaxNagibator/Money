namespace Money.Api.Dto.Accounts;

/// <summary>
/// Запрос на подтверждение электронной почты.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// Код подтверждения.
    /// </summary>
    public required string ConfirmCode { get; init; }
}
