using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;

namespace ServiceRespone.Account
{
    [DataContract]
    public class RegistrationResponse
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public int UserId { get; set; }
    }

    [DataContract]
    public class LoginResponse
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public int UserId { get; set; }
    }

    [DataContract]
    public class ConfirmEmailResponse
    {
    }

    [DataContract]
    public class СhangePasswordResponse
    {
    }

    [DataContract]
    public class GetCommentsResponse
    {
        [DataMember]
        public List<CommentValue> Comments { get; set; }

        [DataContract]
        public class CommentValue
        {
            [DataMember]
            public string Title { get; set; }

            [DataMember]
            public string Text { get; set; }

            [DataMember]
            public string Author { get; set; }

            [DataMember]
            public string CreateDate { get; set; }
        }
    }

    [DataContract]
    public class CreateCommentResponse
    {
    }
}
