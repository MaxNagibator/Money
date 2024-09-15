using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetDebtUsersResponse
    {
        [DataMember]
        public List<DebtUserValue> DebtUsers { get; set; }
        public int TotalCount { get; set; }

        [DataContract]
        public class DebtUserValue
        {
            [DataMember]
            public int Id { get; set; }


            [DataMember]
            public string Name { get; set; }
        }
    }
}
