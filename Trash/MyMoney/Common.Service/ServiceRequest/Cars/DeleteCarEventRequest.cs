using System.Runtime.Serialization;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class DeleteCarEventRequest : TokenRequest
    {
        [DataMember]
        public int CarEventId { get; set; }
    }
}
