namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.DebtUser")]
    public partial class DebtUser
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int DebtUserId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Name { get; set; }

        public virtual User User { get; set; }
    }
}
