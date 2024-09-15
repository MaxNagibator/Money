using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetRegularTasksResponse
    {
        [DataMember]
        public List<TaskValue> Tasks { get; set; }

        [DataContract]
        public class TaskValue
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
            public double DateFrom { get; set; }

            [DataMember]
            public double? DateTo { get; set; }
            [DataMember]
            public double? RunTime { get; set; }

            [DataMember]
            public PaymentValue Payment { get; set; }

            [DataContract]
            public class PaymentValue
            {
                [DataMember]
                public int? CategoryId { get; set; }

                [DataMember]
                public decimal Sum { get; set; }

                [DataMember]
                public PaymentTypes PaymentType { get; set; }

                [DataMember]
                public string Place { get; set; }

                [DataMember]
                public string Comment { get; set; }
            }
        }
    }
}
