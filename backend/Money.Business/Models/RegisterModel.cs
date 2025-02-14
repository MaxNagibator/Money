namespace Money.Business.Models;

public class RegisterModel
{
    public required string UserName { get; set; }
    public string? Email { get; set; }
    public required string Password { get; set; }
}
