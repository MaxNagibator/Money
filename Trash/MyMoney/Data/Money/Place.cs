namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.Place")]
    public partial class Place
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        public DateTime? LastUsedDate { get; set; }

        public int PlaceId { get; set; }

        public virtual User User { get; set; }
    }
}
