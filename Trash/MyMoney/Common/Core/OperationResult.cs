using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Core
{
    public class OperationResult
    {
        public OperationResultTypes Type { get; set; }
        public string Message { get; set; }
    }

    public enum OperationResultTypes
    {
        Success = 1,
        Warning = 2,
        Error = 3,
    }
}
