using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;
using Extentions;

namespace Common
{
    public class Payment
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; }
        
        public decimal Sum { get; set; }

        public PaymentTypes PaymentType { get; set; }

        public string Comment { get; set; }

        public string Place { get; set; }

        public DateTime Date { get; set; }

        public int? CreatedTaskId { get; set; }
    }

    [DataContract]
    public class PaymentStatistics
    {
        [DataMember]
        public List<PaymentCateroryStatistics> CategoryStatistics{get;set;}
    }

    [DataContract]
    public class PaymentCateroryStatistics
    {
        [DataMember]
        public PaymentCategory Category { get; set; }

        [DataMember]
        public decimal Sum { get; set; }

        public List<Guid> PaymentIds { get; set; } 
    }
}
