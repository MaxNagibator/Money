using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Api.Data
{
    public class DomainUser
    {
        [Key]
        [Column]
        public int Id { get; set; }

        [Column]
        public Guid AuthUserId { get; set; }

        public required List<Category> Categories { get; set; }
    }
}
