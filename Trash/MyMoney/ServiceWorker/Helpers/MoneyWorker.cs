using Common.Core;
using DataWorker;

namespace ServiceWorker
{
    public class LogHelper
    {
        public static void AddLog(string message, LogTypes type, string addMessage = null)
        {
            DbLogWorker.AddLog(message, type, addMessage);
        }
    }
}