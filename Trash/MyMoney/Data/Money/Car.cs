namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.Car")]
    public partial class Car
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CarId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Name { get; set; }

        public virtual User User { get; set; }
    }
}
