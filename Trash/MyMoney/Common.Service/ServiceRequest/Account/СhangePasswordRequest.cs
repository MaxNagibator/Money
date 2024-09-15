using System.Runtime.Serialization;

namespace ServiceRequest.Account
{
    [DataContract]
    public class СhangePasswordRequest : TokenRequest
    {
        [DataMember]
        public string OldPassword { get; set; }

        [DataMember]
        public string NewPassword { get; set; }
    }
}
