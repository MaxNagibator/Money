using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class GetCarsRequest : TokenRequest
    {
        [DataMember]
        public List<int> CarIds { get; set; }

        [DataMember]
        public List<string> Fields { get; set; }

        public static class FieldValues
        {
            public const string Events = "events";
        }
    }
}
