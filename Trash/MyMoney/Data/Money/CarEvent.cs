namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.CarEvent")]
    public partial class CarEvent
    {
        public int Id { get; set; }

        public int CarId { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        public int Type { get; set; }

        [StringLength(1000)]
        public string Title { get; set; }

        public string Comment { get; set; }

        public decimal? Mileage { get; set; }

        public DateTime Date { get; set; }

        public virtual User User { get; set; }
    }
}
