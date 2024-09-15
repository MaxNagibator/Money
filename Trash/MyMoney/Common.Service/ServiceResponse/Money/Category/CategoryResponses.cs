using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRespone.Money
{

    [DataContract]
    public class CreateCategoryResponse
    {
        [DataMember]
        public int CategoryId { get; set; }
    }

    [DataContract]
    public class UpdateCategoryResponse
    {

    }

    [DataContract]
    public class DeleteCategoryResponse
    {

    }
}
