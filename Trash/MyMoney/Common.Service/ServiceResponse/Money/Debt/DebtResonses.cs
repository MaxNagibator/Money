using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceRespone.Money
{
    [DataContract]
    public class CreateDebtResponse
    {
        [DataMember]
        public int DebtId { get; set; }
    }

    [DataContract]
    public class PayDebtResponse
    {
    }

    [DataContract]
    public class UpdateDebtResponse
    {
    }

    [DataContract]
    public class DeleteDebtResponse
    {
    }

    [DataContract]
    public class MoveDebtToOperationsResponse
    {
    }

    [DataContract]
    public class MergeDebtUsersResponse
    {
    }
}
