using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceRespone.Money
{
    [DataContract]
    public class CreateFastOperationResponse
    {
        [DataMember]
        public int FastOperationId { get; set; }
    }

    [DataContract]
    public class UpdateFastOperationResponse
    {
    }

    [DataContract]
    public class DeleteFastOperationResponse
    {
    }
}
