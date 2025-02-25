using Money.Data.Entities;

namespace Money.Business;

public class RequestEnvironment
{
    public int? UserId { get; set; }
    public ApplicationUser? AuthUser { get; set; }
}
