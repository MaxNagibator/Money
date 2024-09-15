using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetCategoryResponse
    {
        [DataMember]
        public CategoryValue Category { get; set; }

        [DataContract]
        public class CategoryValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public int? ParentId { get; set; }

            [DataMember]
            public int? Order { get; set; }

            [DataMember]
            public string Color { get; set; }

            [DataMember]
            public PaymentTypes PaymentType { get; set; }
        }
    }
}
