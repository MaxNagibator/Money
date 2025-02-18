using Microsoft.AspNetCore.Identity;

namespace Money.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? EmailConfirmCode { get; set; }
}
