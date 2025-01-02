using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities;

public class DomainUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid AuthUserId { get; set; }

    // TODO: обработать конкурентные изменения
    public int NextCategoryId { get; set; }

    public int NextOperationId { get; set; }

    public int NextPlaceId { get; set; }

    public int NextFastOperationId { get; set; }

    public int NextRegularOperationId { get; set; }

    public int NextDebtId { get; set; }

    public int NextDebtUserId { get; set; }
}
