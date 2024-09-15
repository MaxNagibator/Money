using System.Runtime.Serialization;

namespace ServiceRequest.Account
{
    [DataContract]
    public class LoginRequest
    {
        [DataMember]
        public string Client { get; set; }

        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}
