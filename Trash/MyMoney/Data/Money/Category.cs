namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.Category")]
    public partial class Category
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        public int? ParentId { get; set; }

        [StringLength(100)]
        public string Color { get; set; }

        public int TypeId { get; set; }

        public int? Order { get; set; }

        public virtual User User { get; set; }
    }
}
