namespace Money.Api.Constracts.Accounts
{
    public sealed class AccountRegisterInfo
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}