using System.Runtime.Serialization;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class CreateCarRequest : TokenRequest
    {
        [DataMember]
        public CarValue Car { get; set; }

        [DataContract]
        public class CarValue
        {
            [DataMember]
            public string Name { get; set; }
        }
    }
}
