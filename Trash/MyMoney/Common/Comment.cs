using System;
using System.Runtime.Serialization;

namespace Common
{
    public class Comment
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
