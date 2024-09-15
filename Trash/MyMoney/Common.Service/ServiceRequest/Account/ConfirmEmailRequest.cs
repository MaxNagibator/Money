using System.Runtime.Serialization;

namespace ServiceRequest.Account
{
    [DataContract]
    public class ConfirmEmailRequest
    {
        [DataMember]
        public string Client { get; set; }

        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Code { get; set; }
    }
}
