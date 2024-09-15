using System;
using Common.Enums;

namespace Common
{
    public class Debt
    {
        public int Id { get; set; }

        public DebtTypes Type { get; set; }

        public decimal Sum { get; set; }

        public string Comment { get; set; }

        public DebtUser DebtUser { get; set; }

        public DateTime Date { get; set; }

        public decimal PaySum { get; set; }

        public string PayComment { get; set; }

        public DebtStatus Status { get; set; }
    }
}
