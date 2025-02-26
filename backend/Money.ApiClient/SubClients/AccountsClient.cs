namespace Money.ApiClient;

public class AccountsClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Account";

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
