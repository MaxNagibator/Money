using Common.Enums;
using System.Runtime.Serialization;

namespace ServiceRequest.Money
{
    [DataContract]
    public class GetRegularTasksRequest : TokenRequest
    {
    }

    [DataContract]
    public class GetRegularTaskRequest : TokenRequest
    {
        [DataMember]
        public int RegularTaskId { get; set; }
    }

    [DataContract]
    public class CreateRegularTaskRequest : TokenRequest
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public RegularTaskTimeTypes TimeType { get; set; }

        [DataMember]
        public RegularTaskTypes Type { get; set; }

        [DataMember]
        public int? TimeValue { get; set; }

        [DataMember]
        public string DateFrom { get; set; }

        [DataMember]
        public string DateTo { get; set; }

        [DataMember]
        public PaymentValue Payment { get; set; }

        [DataContract]
        public class PaymentValue
        {
            [DataMember]
            public int CategoryId { get; set; }

            [DataMember]
            public decimal Sum { get; set; }

            [DataMember]
            public string Place { get; set; }

            [DataMember]
            public string Comment { get; set; }
        }
    }

    [DataContract]
    public class UpdateRegularTaskRequest : TokenRequest
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public RegularTaskTimeTypes TimeType { get; set; }

        [DataMember]
        public RegularTaskTypes Type { get; set; }

        [DataMember]
        public int? TimeValue { get; set; }

        [DataMember]
        public string DateFrom { get; set; }

        [DataMember]
        public string DateTo { get; set; }

        [DataMember]
        public PaymentValue Payment { get; set; }

        [DataContract]
        public class PaymentValue
        {
            [DataMember]
            public int CategoryId { get; set; }

            [DataMember]
            public decimal Sum { get; set; }

            [DataMember]
            public string Place { get; set; }

            [DataMember]
            public string Comment { get; set; }
        }
    }

    [DataContract]
    public class DeleteRegularTaskRequest : TokenRequest
    {
        [DataMember]
        public int RegularTaskId { get; set; }
    }

    [DataContract]
    public class RunRegularTaskRequest : TokenRequest
    {
    }
}
