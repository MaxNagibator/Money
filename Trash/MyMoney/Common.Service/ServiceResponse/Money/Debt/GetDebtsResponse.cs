using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetDebtsResponse
    {
        [DataMember]
        public List<DebtValue> Debts { get; set; }

        [DataContract]
        public class DebtValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public DebtTypes Type { get; set; }

            [DataMember]
            public decimal Sum { get; set; }

            [DataMember]
            public string Comment { get; set; }

            [DataMember]
            public DebtUserValue DebtUser { get; set; }

            [DataMember]
            public double Date { get; set; }

            [DataMember]
            public decimal PaySum { get; set; }

            [DataMember]
            public string PayComment { get; set; }

            [DataMember]
            public DebtStatus Status { get; set; }
        }

        [DataContract]
        public class DebtUserValue
        {
            [DataMember]
            public int Id { get; set; }


            [DataMember]
            public string Name { get; set; }
        }
    }
}
