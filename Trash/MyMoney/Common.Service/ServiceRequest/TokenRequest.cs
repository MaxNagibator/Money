using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceRequest
{
    [DataContract]
    public class TokenRequest
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string Client { get; set; }

        //[DataMember]
        //public string Ip { get; set; }
    }
}
