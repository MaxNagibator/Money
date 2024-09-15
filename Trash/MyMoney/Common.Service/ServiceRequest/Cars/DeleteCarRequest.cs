using System.Runtime.Serialization;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class DeleteCarRequest : TokenRequest
    {
        [DataMember]
        public int CarId { get; set; }
    }
}
