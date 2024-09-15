using System;
using Common.Enums;

namespace Common
{
    public class FastOperation
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? CategoryId { get; set; }

        public decimal Sum { get; set; }

        public PaymentTypes PaymentType { get; set; }

        public string Comment { get; set; }

        public string Place { get; set; }

        public int? Order { get; set; }
    }
}
