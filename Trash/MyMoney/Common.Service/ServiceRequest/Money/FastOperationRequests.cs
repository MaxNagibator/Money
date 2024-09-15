using Common.Enums;
using System.Runtime.Serialization;

namespace ServiceRequest.Money
{
    [DataContract]
    public class GetFastOperationsRequest : TokenRequest
    {
    }

    [DataContract]
    public class GetFastOperationRequest : TokenRequest
    {
        [DataMember]
        public int FastOperationId { get; set; }
    }

    [DataContract]
    public class CreateFastOperationRequest : TokenRequest
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public decimal Sum { get; set; }

        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public int? Order { get; set; }
    }

    [DataContract]
    public class UpdateFastOperationRequest : CreateFastOperationRequest
    {
        [DataMember]
        public int Id { get; set; }
    }

    [DataContract]
    public class DeleteFastOperationRequest : TokenRequest
    {
        [DataMember]
        public int FastOperationId { get; set; }
    }
}
