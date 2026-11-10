namespace Money.Business.Configs;

public class AccountSettings
{
    public int MaxConfirmEmailFailedAttempts { get; init; } = 5;
    public TimeSpan ConfirmEmailLockoutDuration { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan ResendConfirmCodeCooldown { get; init; } = TimeSpan.FromMinutes(1);
}
