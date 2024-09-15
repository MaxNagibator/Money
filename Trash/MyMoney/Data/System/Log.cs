namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("System.Log")]
    public partial class Log
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public int TypeId { get; set; }

        [Required]
        [StringLength(2048)]
        public string Message { get; set; }

        public string AdditionalMessage { get; set; }

        [StringLength(100)]
        public string Ip { get; set; }

        public int? ClientTypeId { get; set; }

        public virtual User User { get; set; }
    }
}
