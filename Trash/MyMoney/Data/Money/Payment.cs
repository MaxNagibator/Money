namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.Payment")]
    public partial class Payment
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int PaymentId { get; set; }

        public decimal Sum { get; set; }

        public int? CategoryId { get; set; }

        public int TypeId { get; set; }

        [StringLength(4000)]
        public string Comment { get; set; }

        public DateTime Date { get; set; }

        public int? TaskId { get; set; }

        public int? CreatedTaskId { get; set; }

        public int? PlaceId { get; set; }

        public virtual User User { get; set; }
    }
}
