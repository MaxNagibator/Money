using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRespone.Cars
{
    [DataContract]
    public class CreateCarEventResponse
    {
        [DataMember]
        public int CarEventId { get; set; }
    }
}
