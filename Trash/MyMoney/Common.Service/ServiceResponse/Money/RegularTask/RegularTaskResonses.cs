using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceRespone.Money
{
    [DataContract]
    public class CreateRegularTaskResponse
    {
        [DataMember]
        public int RegularTaskId { get; set; }
    }

    [DataContract]
    public class UpdateRegularTaskResponse
    {
    }

    [DataContract]
    public class DeleteRegularTaskResponse
    {
    }

    [DataContract]
    public class RunRegularTaskResponse
    {
    }
}
