using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Core
{
    public enum LogTypes
    {
        [Description("Info")]
        Info = 1,

        [Description("Error")]
        Error = 2,

        [Description("Warning")]
        Warning = 3,

        [Description("Success")]
        Success = 4,

        [Description("Debug")]
        Debug = 5,
    }
}
