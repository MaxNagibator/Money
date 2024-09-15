using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using Extentions;
using Newtonsoft.Json;

namespace MyMoneyWeb.Controllers
{
    public class DevController : BaseController
    {
        [HttpPost]
        public ActionResult Post(string service, string method, string[] args)
        {
            var serviceClassName = service.FirstUpper() + "Worker";
            Type serviceClass = ServiceWorker.Api.ApiHelper.GetType(serviceClassName);
            if (serviceClass == null)
            {
                return Json(new { success = false, message = "service don't recognize", service = service }, JsonRequestBehavior.AllowGet);
            }
            MethodInfo serviceMethod = serviceClass.GetMethod(method);
            if (serviceMethod == null)
            {
                return Json(new { success = false, message = "method don't recognize", service = service, method = method }, JsonRequestBehavior.AllowGet);
            }

            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            var methodParams = serviceMethod.GetParameters();
            var methodParamsObj = new List<object>();
            var param0 = methodParams[0];
            var paramType = param0.ParameterType;
            var convertedParam = JsonConvert.DeserializeObject(json, paramType);
            methodParamsObj.Add(convertedParam);
            methodParamsObj.Add(Request);
            var r = serviceMethod.Invoke(this, methodParamsObj.ToArray());
            var str = JsonConvert.SerializeObject(r);
            return new JsonStringResult(str);
        }

        public class JsonStringResult : ContentResult
        {
            public JsonStringResult(string json)
            {
                Content = json;
                ContentType = "application/json";
            }
        }

        [HttpGet]
        [ValidateInput(false)]
        public ActionResult Get(string service, string method, string[] args)
        {
            var serviceClassName = service.FirstUpper() + "Worker";
            Type serviceClass = ServiceWorker.Api.ApiHelper.GetType(serviceClassName);
            if(serviceClass == null)
            {
                return Json(new { success = false, message = "service don't recognize", service = service }, JsonRequestBehavior.AllowGet);
            }
            MethodInfo serviceMethod = serviceClass.GetMethod(method);
            if (serviceMethod == null)
            {
                return Json(new { success = false, message = "method don't recognize", service = service , method = method }, JsonRequestBehavior.AllowGet);
            }
            var methodParams = serviceMethod.GetParameters();
            var methodParamsObj = new List<object>();
            var param0 = methodParams[0];
            var paramType = param0.GetType();
            var convertedParam = JsonConvert.DeserializeObject(args[0], paramType);
            methodParamsObj.Add(convertedParam);
            var r = serviceMethod.Invoke(this, methodParamsObj.ToArray());
            var str = JsonConvert.SerializeObject(r);
            return Json(new { str }, JsonRequestBehavior.AllowGet);
        }
    }
}
