using System.ComponentModel.DataAnnotations;

namespace Money.Data.Entities;

public class DomainUser
{
    [Key]
    public int Id { get; set; }

    public Guid AuthUserId { get; set; }

    public List<Category>? Categories { get; set; }
}
