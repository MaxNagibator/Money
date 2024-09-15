using ClosedXML.Excel;
using Common.Enums;
using Extentions;
using MyMoneyWeb.Models;
using MyMoneyWeb.Structure;
using NCalc;
using ServiceRequest.Money;
using ServiceResponse;
using ServiceWorker;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MyMoneyWeb.Controllers
{
    public class FinanceFastOperationsController : BaseController
    {
        [HttpPost]
        public ActionResult GetFastOperations()
        {
            var token = Request.GetAuthToken();
            var getFastOperationsRequest = new GetFastOperationsRequest();
            getFastOperationsRequest.Token = token;
            var response = MoneyWorker.GetFastOperations(getFastOperationsRequest, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }
            var model = new FastOperationsModel();
            var m = FinanceController.GetCategoriesList(Request, null);
            model.PaymentCategories = m.Categories;
            model.FastOperations = FinanceController.GetFastOperations(Request);
            return PartialView("FastOperations", model);
        }

        [HttpPost]
        public ActionResult GetFastOperationForm(int? fastOperationId = null)
        {
            var token = Request.GetAuthToken();
            var request = new GetCategoriesRequest();
            request.Token = token;
            var m = FinanceController.GetCategoriesList(Request, null);
            if (m.Categories == null)
            {
                return Json(new { success = false, message = m.Message });
            }
            var model = new FastOperationFormModel();
            model.PaymentCategories = m.Categories;

            if (fastOperationId != null)
            {
                var getFastOperationRequest = new GetFastOperationRequest();
                getFastOperationRequest.Token = token;
                getFastOperationRequest.FastOperationId = fastOperationId.Value;
                var response = MoneyWorker.GetFastOperation(getFastOperationRequest, Request);
                if (response.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = m.Message });
                }
                var fastOperation = response.Body.FastOperation;

                var fastOperationModel = new FastOperationModel();

                fastOperationModel.Id = fastOperation.Id;
                fastOperationModel.PaymentType = fastOperation.PaymentType;
                fastOperationModel.Name = fastOperation.Name;
                fastOperationModel.Order = fastOperation.Order;
                fastOperationModel.Sum = fastOperation.Sum;
                fastOperationModel.CategoryId = fastOperation.CategoryId;
                fastOperationModel.Comment = fastOperation.Comment;
                fastOperationModel.Place = fastOperation.Place;

                model.FastOperation = fastOperationModel;
            }
            else
            {
                model.FastOperation = new FastOperationModel();
            }

            return PartialView("FastOperationForm", model);
        }

        [HttpPost]
        public ActionResult SaveFastOperation(FastOperationFormModel form)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("FastOperationForm", form);
            }

            var token = Request.GetAuthToken();
            var fastOperation = form.FastOperation;
            if (fastOperation.Id == null)
            {
                var createFastOperationRequest = new CreateFastOperationRequest();
                createFastOperationRequest.Token = token;
                createFastOperationRequest.Name = fastOperation.Name;
                createFastOperationRequest.Order = fastOperation.Order;
                createFastOperationRequest.Sum = fastOperation.Sum.Value;
                createFastOperationRequest.CategoryId = fastOperation.CategoryId.Value;
                createFastOperationRequest.Comment = fastOperation.Comment;
                createFastOperationRequest.Place = fastOperation.Place;

                var createResponse = MoneyWorker.CreateFastOperation(createFastOperationRequest, Request);
                if (createResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = createResponse.ResponseMessage });
                }
                fastOperation.Id = createResponse.Body.FastOperationId;
            }
            else
            {
                var updateFastOperationRequest = new UpdateFastOperationRequest();
                updateFastOperationRequest.Token = token;
                updateFastOperationRequest.Id = fastOperation.Id.Value;
                updateFastOperationRequest.Name = fastOperation.Name;
                updateFastOperationRequest.Order = fastOperation.Order;
                updateFastOperationRequest.Sum = fastOperation.Sum.Value;
                updateFastOperationRequest.CategoryId = fastOperation.CategoryId.Value;
                updateFastOperationRequest.Comment = fastOperation.Comment;
                updateFastOperationRequest.Place = fastOperation.Place;

                var createResponse = MoneyWorker.UpdateFastOperation(updateFastOperationRequest, Request);
                if (createResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = createResponse.ResponseMessage });
                }
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult DeleteFastOperation(int fastOperationId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteFastOperationRequest();
            request.Token = token;
            request.FastOperationId = fastOperationId;
            var payDebtResponse = MoneyWorker.DeleteFastOperation(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }
    }
}
