namespace Money.Api.Dto.Accounts;

public class ConfirmEmailRequest
{
    public required string ConfirmCode { get; init; }
}
