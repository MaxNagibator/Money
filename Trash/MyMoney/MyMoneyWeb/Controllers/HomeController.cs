using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMoneyWeb.Helpers;
using MyMoneyWeb.Models;
using MyMoneyWeb.Structure;
using ServiceRequest.Money;
using ServiceResponse;
using ServiceWorker;

namespace MyMoneyWeb.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Operation()
        {
            var token = Request.GetAuthToken();
            //var getCategoriesRequest = new GetCategoriesRequest();
            //getCategoriesRequest.Token = token;
            //var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            //if (getCategoriesResponse.Type != ResponseType.Success)
            //{
            //    return Json(new { success = false, message = getCategoriesResponse.ResponseMessage });
            //}
            var model = new OperationModel();
            //model.Categories = new List<CategoryModel>();
            //foreach (var category in getCategoriesResponse.Body.Categories)
            //{
            //    var c = new CategoryModel();
            //    c.Id = category.Id;
            //    c.Color = category.Color;
            //    c.Name = category.Name;
            //    c.Order = category.Order;
            //    c.ParentId = category.ParentId;
            //    model.Categories.Add(c);
            //}

            var m = FinanceController.GetCategoriesList(Request, null);
            if (m.Categories == null)
            {
                return Json(new { success = false, message = m.Message });
            }
            model.Categories = m.Categories;
            model.FastOperations = FinanceController.GetFastOperations(Request);

            return View("Operation", model);
        }

        [Authorize]
        public ActionResult Category()
        {
            return View("Category");
        }

        [Authorize]
        public ActionResult Debt()
        {
            return View("Debt");
        }

        [Authorize]
        public ActionResult Car()
        {
            return View("Car");
        }

        [Authorize]
        public ActionResult DocumentTemplateGroup()
        {
            return View("DocumentTemplateGroup");
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Mobile()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            //var xxx = ServiceWorker.MoneyWorker.GetCategories(new GetCategoriesRequest());
            //var message = (int)xxx.Status;
            //ViewBag.Message = message;
            return View();
        }

        public ActionResult Comment()
        {
            return View();
        }
    }
}
