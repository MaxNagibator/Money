using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetPlacesResponse
    {
        [DataMember]
        public List<PlaceValue> Places { get; set; }

        [DataContract]
        public class PlaceValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }
        }
    }
}
