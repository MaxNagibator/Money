namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Money.Debt")]
    public partial class Debt
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int DebtId { get; set; }

        public DateTime Date { get; set; }

        public decimal Sum { get; set; }

        public int Type { get; set; }

        public string Comment { get; set; }

        public decimal PaySum { get; set; }

        public int StatusId { get; set; }

        public string PayComment { get; set; }

        public int DebtUserId { get; set; }

        public virtual User User { get; set; }
    }
}
