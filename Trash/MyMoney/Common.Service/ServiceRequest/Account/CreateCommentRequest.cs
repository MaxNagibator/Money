using System.Runtime.Serialization;

namespace ServiceRequest.Account
{

    [DataContract]
    public class CreateCommentRequest : TokenRequest
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Author { get; set; }
    }
}
