using System.Runtime.Serialization;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class UpdateCarRequest : TokenRequest
    {
        [DataMember]
        public CarValue Car { get; set; }

        [DataContract]
        public class CarValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }
        }
    }
}
