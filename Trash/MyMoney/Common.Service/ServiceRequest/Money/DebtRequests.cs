using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common;
using Common.Enums;

namespace ServiceRequest.Money
{
    [DataContract]
    public class GetDebtsRequest : TokenRequest
    {
        [DataMember]
        public bool WithPaid { get; set; }
    }

    [DataContract]
    public class GetDebtRequest : TokenRequest
    {
        [DataMember]
        public int DebtId { get; set; }
    }

    [DataContract]
    public class CreateDebtRequest : TokenRequest
    {
        [DataMember]
        public decimal Sum { get; set; }
        [DataMember]
        public DebtTypes Type { get; set; }
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }

    [DataContract]
    public class PayDebtRequest : TokenRequest
    {
        [DataMember]
        public int DebtId { get; set; }
        [DataMember]
        public decimal Sum { get; set; }
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }

    [DataContract]
    public class UpdateDebtRequest : TokenRequest
    {
        [DataMember]
        public int DebtId { get; set; }

        [DataMember]
        public decimal Sum { get; set; }

        [DataMember]
        public string Date { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }

    [DataContract]
    public class DeleteDebtRequest : TokenRequest
    {
        [DataMember]
        public int DebtId { get; set; }
    }

    [DataContract]
    public class MoveDebtToOperationsRequest : TokenRequest
    {
        [DataMember]
        public List<int> DebtIds { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }

    [DataContract]
    public class GetDebtUsersRequest : TokenRequest
    {
        public string Search { get; set; }
    }

    [DataContract]
    public class MergeDebtUsersRequest : TokenRequest
    {
        [DataMember]
        public int FromUserId { get; set; }

        [DataMember]
        public int ToUserId { get; set; }
    }
}
