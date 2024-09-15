using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceWorker;

namespace MyMoneyWeb.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var accessIps = new List<string>() { "127.0.0.1", "::1", "78.139.224.230", "10.177.0.31" };
            string ip = Request.UserHostAddress;
            //if (!accessIps.Contains(ip))
            //{
            //    throw new Exception("Access denied by IP "+ ip);
            //}
            base.OnActionExecuting(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Exception exception = filterContext.Exception;
            filterContext.ExceptionHandled = true;
            var action = filterContext.RouteData.Values["action"].ToString();
            var type = filterContext.Controller.GetType();
            var method = type.GetMethod(action);
            var returnType = method.ReturnType;

            var msg = "ошибка на сервере";

            LogHelper.AddLog(exception.Message, Common.Core.LogTypes.Error, exception.StackTrace);
            //var returnTypeAttr = ReturnType.View;
            //var statusAttributes = method.GetCustomAttributes(typeof(ExceptionReturnTypeAttribute), true);
            //if (statusAttributes.Length > 0)
            //{
            //    returnTypeAttr = ((ExceptionReturnTypeAttribute)statusAttributes[0]).Type;
            //}

            //if (returnTypeAttr == ReturnType.Json)
            //{
            //    filterContext.Result = Json(new { success = false, message = msg });
            //}
            //else
            //{
            ViewBag.MessageTitle = "Внимание";
            ViewBag.Message = exception.Message;
            filterContext.Result = PartialView("Error");
            //  }
            filterContext.ExceptionHandled = true;
        }
    }
}
