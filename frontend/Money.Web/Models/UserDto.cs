namespace Money.Web.Models;

public record RegisterUserDto(string UserName, string? Email, string Password);
public record UserDto(string Login, string Password);
