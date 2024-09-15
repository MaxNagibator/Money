namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.FastOperation")]
    public partial class FastOperation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int FastOperationId { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal Sum { get; set; }

        public int? CategoryId { get; set; }

        public int TypeId { get; set; }

        [StringLength(4000)]
        public string Comment { get; set; }

        public int? PlaceId { get; set; }

        public int? Order { get; set; }

        public virtual User User { get; set; }
    }
}
