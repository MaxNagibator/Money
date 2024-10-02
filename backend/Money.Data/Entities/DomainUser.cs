using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities;

public class DomainUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid AuthUserId { get; set; }

    public int NextCategoryId { get; set; }

    public int NextPaymentId { get; set; }

    public int NextPlaceId { get; set; }

    public List<Category>? Categories { get; set; }
}
