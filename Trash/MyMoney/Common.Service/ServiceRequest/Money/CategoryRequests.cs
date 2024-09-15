using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRequest.Money
{
    [DataContract]
    public class DeleteCategoryRequest : TokenRequest
    {
        [DataMember]
        public int CategoryId { get; set; }
    }

    [DataContract]
    public class GetCategoryRequest : TokenRequest
    {
        [DataMember]
        public int CategoryId { get; set; }
    }

    [DataContract]
    public class GetCategoriesRequest : TokenRequest
    {
        [DataMember]
        public PaymentTypes? PaymentType { get; set; }
    }
    [DataContract]
    public class UpdateCategoryRequest : TokenRequest
    {
        [DataMember]
        public int CategoryId { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int? ParentId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int? Order { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
    [DataContract]
    public class CreateCategoryRequest : TokenRequest
    {
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public int? ParentId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int? Order { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public PaymentTypes PaymentType { get; set; }
    }
}
