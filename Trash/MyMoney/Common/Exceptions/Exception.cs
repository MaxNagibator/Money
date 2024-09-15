using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class BaseException : Exception
    {
        public string LogMessage { get; set; }
        public int? Code { get; set; }
        public BaseException() { }
        public BaseException(string message, string logMessage = null, int? code = null)
            : base(message)
        {
            if (logMessage == null)
            {
                logMessage = message;
            }
            LogMessage = logMessage;
            Code = code;
        }
    }

    public class MessageException : BaseException
    {
        public MessageException(string message, string logMessage = null, int? code = null)
            : base(message, logMessage, code) { }
    }
}
