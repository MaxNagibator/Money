namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.RegularTask")]
    public partial class RegularTask
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int TaskId { get; set; }

        [Required]
        public string Name { get; set; }

        public int TypeId { get; set; }

        public int TimeId { get; set; }

        public int? TimeValue { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateTo { get; set; }

        public DateTime? RunTime { get; set; }

        public virtual User User { get; set; }
    }
}
