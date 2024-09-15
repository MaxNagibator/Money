using System.Runtime.Serialization;

namespace ServiceRespone.Money
{
    [DataContract]
    public class CreatePaymentResponse
    {
        [DataMember]
        public int PaymentId { get; set; }
    }

    [DataContract]
    public class UpdatePaymentResponse
    {

    }

    [DataContract]
    public class UpdatePaymentsBatchResponse
    {

    }
    

    [DataContract]
    public class DeletePaymentResponse
    {

    }

    [DataContract]
    public class GetPaymentStatisticsResponse
    {
    }
}
