namespace Money.ApiClient;

public class AccountsClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Accounts";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse> RegisterAsync(RegisterRequest request)
    {
        return PostAsync($"{BaseUri}/register", request);
    }

    public Task<ApiClientResponse> ConfirmEmailAsync(string code)
    {
        return PostAsync($"{BaseUri}/ConfirmEmail", new ConfirmEmailRequest { ConfirmCode = code });
    }

    public Task<ApiClientResponse> ResendConfirmCodeAsync()
    {
        return PostAsync($"{BaseUri}/ResendConfirmCode");
    }

    public Task<ApiClientResponse> ChangePassword(string currentPassword, string newPassword)
    {
        return PostAsync($"{BaseUri}/ChangePassword", new { CurrentPassword = currentPassword, NewPassword = newPassword });
    }

    public class RegisterRequest
    {
        public required string UserName { get; set; }
        public string? Email { get; set; }
        public required string Password { get; set; }
    }
    public class ConfirmEmailRequest
    {
        public required string ConfirmCode { get; set; }
    }
}
