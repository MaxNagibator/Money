using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class GetCarEventsRequest : TokenRequest
    {
        [DataMember]
        public List<int> CarEventIds { get; set; }
    }
}
