using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRespone.Cars
{
    [DataContract]
    public class CreateCarResponse
    {
        [DataMember]
        public int CarId { get; set; }
    }
}
