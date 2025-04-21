namespace Money.Api.Dto.Accounts;

public class ChangePasswordRequest
{
    public required string CurrentPassword { get; set; }

    public required string NewPassword { get; set; }
}
