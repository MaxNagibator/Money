namespace Money.Api.Tests;

public class ApiUser
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public string? Token { get; set; }
}
