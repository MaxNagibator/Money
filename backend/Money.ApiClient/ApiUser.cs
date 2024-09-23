namespace Money.ApiClient;

public class ApiUser
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public AuthData? AuthData { get; set; }
    public string? Token => AuthData?.AccessToken;
}
