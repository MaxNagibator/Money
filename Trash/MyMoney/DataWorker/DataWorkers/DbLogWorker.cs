using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Common.Service;

namespace DataWorker
{
    public class DbLogWorker
    {
        public static int? GetUserIdByToken(string token)
        {
            using (var context = new Data.DataContext())
            {
                var dbUser = context.Users.SingleOrDefault(x => x.Token == token);
                if (dbUser == null)
                {
                    return null;
                }
                return dbUser.Id;
            }
        }

        public static void WriteExecuteMessage(int userId, string message, LogTypes type, List<ServiceParam> parameters = null, string addMessage = null)
        {
            using (var context = new Data.DataContext())
            {
                string ip = null;
                int? clientTypeId = null;
                if (parameters != null && parameters.Count > 0)
                {
                    var ipParam = parameters.FirstOrDefault(x => x.Name == ServiceParamSystemNames.Ip);
                    if (ipParam != null)
                    {
                        ip = ipParam.Value;
                        clientTypeId = (int)ServiceClientTypes.Classic;
                        parameters.Remove(ipParam);
                    }
                    else
                    {
                        ipParam = parameters.FirstOrDefault(x => x.Name == ServiceParamSystemNames.IpFromWeb);
                        if (ipParam != null)
                        {
                            ip = ipParam.Value;
                            clientTypeId = (int)ServiceClientTypes.Web;
                            parameters.Remove(ipParam);
                        }
                    }
                }
                var parametersString = addMessage;
                if (parameters != null && parameters.Count > 0)
                {
                    if (!String.IsNullOrEmpty(parametersString))
                    {
                        parametersString += Environment.NewLine;
                    }
                    parametersString += "params: ";
                    foreach (var serviceParam in parameters)
                    {
                        parametersString += Environment.NewLine + serviceParam.Name + ": " + serviceParam.Value;
                    }
                }

                var log = new Data.Log
                {
                    Message = message,
                    UserId = userId,
                    TypeId = (int)type,
                    Date = DateTime.Now,
                    AdditionalMessage = parametersString,
                    Ip = ip,
                    ClientTypeId = clientTypeId,
                };
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }

        public static void AddLog(string message, LogTypes type, string addMessage = null)
        {
            using (var context = new Data.DataContext())
            {
                var log = new Data.Log
                {
                    Message = message,
                    UserId = 2,
                    TypeId = (int)type,
                    Date = DateTime.Now,
                    AdditionalMessage = addMessage,
                };
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }

    }
}
