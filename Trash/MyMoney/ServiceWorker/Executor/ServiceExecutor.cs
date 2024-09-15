using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
//using System.ServiceModel;
//using System.ServiceModel.Channels;
using Common.Core;
using Common.Exceptions;
using Common.Service;
using DataWorker;
using Extentions;
using ServiceRequest;
using ServiceResponse;

namespace ServiceWorker.Executor
{
    public static class RequestHelper
    {
        public static int GetUserIdByToken(this TokenRequest request)
        {
            return DbAccountWorker.GetUserIdByToken(request.Token);
        }
    }

    public static class CheckParams
    {
        public static void CheckMoreZero(decimal? value, string param = null)
        {
            if (value != null)
            {
                if (value <= 0)
                {
                    var pText = "";
                    if (param != null)
                    {
                        pText = " " + param;
                    }
                    throw new MessageException("Значение параметра" + pText + " должно быть больше 0");
                }
            }
        }
    }

    public class ServiceExecutor
        {
            public static Response<T> Execute<T, T2>(Func<T> action, HttpRequestBase httpRequestBase, TokenRequest request, T2 serviceInfo, List<ServiceParam> parameters = null)
            {
                var token = request == null ? null : request.Token;
                var client = request == null ? null : request.Client;
                return Execute(action, httpRequestBase, token, client, serviceInfo, parameters);//, request.Ip);
            }

        public static Response<T> Execute<T, T2>(Func<T> action, HttpRequestBase httpRequestBase, string token, string client, T2 serviceInfo, List<ServiceParam> parameters = null)//, string ip = null)
            {

                if (parameters == null)
                {
                    parameters = new List<ServiceParam>();
                }

                if (httpRequestBase != null)
                {
                    var ip = IpHelper.GetRemoteIP(httpRequestBase.ServerVariables);
                    parameters.Add(new ServiceParam(ServiceParamSystemNames.IpFromWeb, ip));
                }
                    parameters.Add(new ServiceParam(ServiceParamSystemNames.Client, client));

                var serviceName = serviceInfo.ServiceNameAttr();
                var methodDescription = serviceInfo.DescriptionAttr();
                var desc = serviceInfo.GetType().DescriptionAttr();
                return Execute(action, token, serviceName, desc, methodDescription, parameters);
            }

            private static Response<T> Execute<T>(Func<T> action, string token, string serviceName,
                string apiDescription, string methodDescription, List<ServiceParam> parameters = null)
            {
                var watcher = new Stopwatch();
                watcher.Start();
                var userId = DbUserWorker.GetUserIdByToken(token);
                var response = GetExecutedValue(action, userId, serviceName, apiDescription, methodDescription, watcher, parameters);
                return response;
            }

            private static Response<T> GetExecutedValue<T>(Func<T> action, int? userId,
                string serviceName, string apiDescription, string methodDescription, Stopwatch watcher, List<ServiceParam> parameters = null)
            {
                var serviceMessage = "Сервис " + apiDescription + "." + methodDescription;
                //if (!String.IsNullOrEmpty(token))
                //{
                //    bool? isBlocked;
                //    var isOwnerExists = DbUserWorker.IsUserExists(token, out isBlocked);
                //    if (!isOwnerExists)
                //    {
                //        success = false;
                //        DbLogsWorker.WriteExecuteMessage(Guid.Empty, apiDescription, methodDescription,
                //            LogsMessageType.Warning, serviceName, watcher, AccessMessage, parameters: parameters);
                //        return Response<T>.GeneratePackage(apiDescription, default(T), PackageStatus.ACCESS_DENIED);
                //    }
                //    if (isBlocked == true)
                //    {
                //        success = false;
                //        var blockedOwnerGuid = DbEmployeeWorker.GetRecordOwnerGuidByToken(token);
                //        DbLogsWorker.WriteExecuteMessage(blockedOwnerGuid, apiDescription, methodDescription,
                //            LogsMessageType.Warning, serviceName, watcher, AccessMessage, parameters: parameters);
                //        return Response<T>.GeneratePackage(apiDescription, default(T), PackageStatus.ACCESS_DENIED);
                //    }
                //}
                try
                {
                    //var accessIps = new List<string>() { "127.0.0.1", "78.139.224.230" };
                    //if (!accessIps.Contains(ip))
                    //{
                    //    watcher.Stop();
                    //    var execiteTime = (int)watcher.ElapsedMilliseconds;
                    //    var message = serviceMessage + " В доступе по IP отказано /" + execiteTime + "ms";
                    //    DbLogWorker.WriteExecuteMessage(userId ?? 2, message, LogTypes.Warning, parameters);
                    //    return Response<T>.GeneratePackage(apiDescription, default(T), ResponseStatus.ACCESS_DENIED_BY_IP, methodDescription);
                    //}

                    var isHasAccess = userId != null
                        || serviceName == ServiceNames.MoneyService.RunRegularTask.ServiceNameAttr()
                        || serviceName == ServiceNames.Account.Login.ServiceNameAttr()
                        || serviceName == ServiceNames.Account.ConfirmEmail.ServiceNameAttr()
                        || serviceName == ServiceNames.Account.Registration.ServiceNameAttr()
                        || serviceName == ServiceNames.Account.GetComments.ServiceNameAttr()
                        || serviceName == ServiceNames.Account.CreateComment.ServiceNameAttr();// DbUmsServicesWorker.IsInRoleByRecordOwnerGuid(ownerGuid, serviceName, methodDescription);
                    if (isHasAccess)
                    {
                        var actionResult = action();

                        watcher.Stop();
                        var execiteTime = (int)watcher.ElapsedMilliseconds;
                        var message = serviceMessage + " /" + execiteTime + "ms";
                        DbLogWorker.WriteExecuteMessage(userId ?? 2, message, LogTypes.Success, parameters);

                        return Response<T>.GeneratePackage(apiDescription, actionResult, ResponseStatus.OK);
                    }
                    else
                    {
                        watcher.Stop();
                        var execiteTime = (int)watcher.ElapsedMilliseconds;
                        var message = serviceMessage + " В доступе отказано /" + execiteTime + "ms";
                        DbLogWorker.WriteExecuteMessage(userId ?? 2, message, LogTypes.Warning, parameters);

                        return Response<T>.GeneratePackage(apiDescription, default(T), ResponseStatus.ACCESS_DENIED, methodDescription);
                    }
                }
                catch (MessageException ex)
                {
                    watcher.Stop();
                    var execiteTime = (int)watcher.ElapsedMilliseconds;
                    var message = serviceMessage + " Невозможная операция /" + execiteTime + "ms";
                    DbLogWorker.WriteExecuteMessage(userId ?? 2, message, LogTypes.Warning, parameters, ex.Message);

                    return Response<T>.GeneratePackage(apiDescription, default(T), ResponseStatus.IMPOSIBLE, ex.Message, ex.Code);
                }
#if (!DEBUG)
            catch (Exception ex)
            {
                var addmessage = ex.ToString();
                watcher.Stop();
                var execiteTime = (int)watcher.ElapsedMilliseconds;
                var message = serviceMessage + " Ошибка /" + execiteTime + "ms";
                DbLogWorker.WriteExecuteMessage(userId ?? 2, message, LogTypes.Warning, parameters, addmessage);

                return Response<T>.GeneratePackage(apiDescription, default(T), ResponseStatus.INTERNAL_ERROR, "Ошибка на сервисе");
            }
#endif
        }
    }
}
