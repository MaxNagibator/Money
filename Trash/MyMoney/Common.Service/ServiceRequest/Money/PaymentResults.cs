using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRequest.Money
{
    [DataContract]
    public class CreatePaymentRequest : TokenRequest
    {
        [DataMember]
        public decimal Sum { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public string Date { get; set; }
    }

    [DataContract]
    public class UpdatePaymentRequest : TokenRequest
    {
        [DataMember]
        public int PaymentId { get; set; }
        [DataMember]
        public decimal Sum { get; set; }
        [DataMember]
        public int CategoryId { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string Place { get; set; }
        [DataMember]
        public string Date { get; set; }
    }


    [DataContract]
    public class UpdatePaymentsBatchRequest : TokenRequest
    {
        [DataMember]
        public List<int> PaymentIds { get; set; }

        [DataMember]
        public int CategoryId { get; set; }
    }

    [DataContract]
    public class DeletePaymentRequest : TokenRequest
    {
        [DataMember]
        public int PaymentId { get; set; }
    }

    [DataContract]
    public class GetPaymentRequest : TokenRequest
    {
        [DataMember]
        public int PaymentId { get; set; }
    }

    [DataContract]
    public class GetPaymentsRequest : TokenRequest
    {
        [DataMember]
        public string DateFrom { get; set; }

        [DataMember]
        public string DateTo { get; set; }

        [DataMember]
        public int DateFromPayment { get; set; }

        [DataMember]
        public List<int> CategoryIds { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string Place { get; set; }
    }

    [DataContract]
    public class GetPaymentStatisticsRequest : TokenRequest
    {
        [DataMember]
        public string DateFrom { get; set; }
        [DataMember]
        public string DateTo { get; set; }
    }

    [DataContract]
    public class GetPlacesRequest : TokenRequest
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Offset { get; set; }

        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public string SortBy { get; set; }
    }
}
