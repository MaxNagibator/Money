using System.ComponentModel.DataAnnotations;

namespace Money.Api.Data;

public class DomainUser
{
    [Key]
    public int Id { get; set; }

    public Guid AuthUserId { get; set; }

    public List<Category>? Categories { get; set; }
}
