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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyMoneyWeb.Controllers
{
    public class FinanceController : BaseController
    {
        [HttpPost]
        public ActionResult GetPayments(int? month = null, string from = null, string to = null, int[] categoryIds = null, string comment = null, string place = null)
        {
            var categoryIdsList = categoryIds.MvcArrayToList();
            var token = Request.GetAuthToken();
            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = getCategoriesResponse.ResponseMessage });
            }
            var fromStr = from.ToNullableDateTime(null);
            var toStr = to.ToNullableDateTime(null);
            DateTime dateFrom;
            DateTime dateTo;
            if (fromStr == null)
            {
                if (toStr == null)
                {
                    var shift = month ?? 0;
                    dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    dateFrom = dateFrom.AddMonths(shift);
                    dateTo = dateFrom.AddMonths(1);
                }
                else
                {
                    dateTo = toStr.Value;
                    dateFrom = dateTo.AddMonths(-1);
                }
            }
            else
            {
                dateFrom = fromStr.Value;
                if (toStr == null)
                {
                    dateTo = dateFrom.AddMonths(1);
                }
                else
                {
                    dateTo = toStr.Value;
                }
            }
            var getPaymentsRequest = new GetPaymentsRequest();
            getPaymentsRequest.Token = token;
            getPaymentsRequest.DateTo = dateTo.ToString();
            getPaymentsRequest.DateFrom = dateFrom.ToString();
            getPaymentsRequest.CategoryIds = categoryIdsList;
            getPaymentsRequest.Comment = comment;
            getPaymentsRequest.Place = place;
            var getPaymentsResponse = MoneyWorker.GetPayments(getPaymentsRequest, Request);
            if (getPaymentsResponse.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = getPaymentsResponse.ResponseMessage });
            }
            var model = new PaymentsModel();
            model.Payments = new List<PaymentModel>();
            foreach (var payment in getPaymentsResponse.Body.Payments)
            {
                var p = new PaymentModel();
                p.Id = payment.Id;
                p.CategoryId = payment.CategoryId;
                p.Comment = payment.Comment;
                p.Place = payment.Place;
                p.CreatedTaskId = payment.CreatedTaskId;
                p.Date = payment.Date.ToDateTime();
                p.Sum = payment.Sum.ToString();
                p.PaymentType = payment.PaymentType;
                model.Payments.Add(p);
            }

            model.Categories = new List<CategoryModel>();
            foreach (var categoryValue in getCategoriesResponse.Body.Categories)
            {
                var c = new CategoryModel();
                c.Id = categoryValue.Id;
                c.Color = categoryValue.Color;
                c.Name = categoryValue.Name;
                c.Order = categoryValue.Order;
                c.ParentId = categoryValue.ParentId;
                c.PaymentTypeId = (int)categoryValue.PaymentType;
                model.Categories.Add(c);
            }

            model.MinDate = dateFrom.ToShortDateString();
            model.MaxDate = dateTo.AddDays(-1).ToShortDateString();
            return PartialView("Payments", model);
        }

        [HttpPost]
        public ActionResult GetPaymentsExcel(int? month = null, string from = null, string to = null, int[] categoryIds = null, string comment = null, string place = null)
        {
            var token = Request.GetAuthToken();
            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = getCategoriesResponse.ResponseMessage });
            }
            var fromStr = from.ToNullableDateTime(null);
            var toStr = to.ToNullableDateTime(null);
            DateTime dateFrom;
            DateTime dateTo;
            if (fromStr == null)
            {
                if (toStr == null)
                {
                    var shift = month ?? 0;
                    dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    dateFrom = dateFrom.AddMonths(shift);
                    dateTo = dateFrom.AddMonths(1);
                }
                else
                {
                    dateTo = toStr.Value;
                    dateFrom = dateTo.AddMonths(-1);
                }
            }
            else
            {
                dateFrom = fromStr.Value;
                if (toStr == null)
                {
                    dateTo = dateFrom.AddMonths(1);
                }
                else
                {
                    dateTo = toStr.Value;
                }
            }

            var categoryIdsList = categoryIds.MvcArrayToList();
            var getPaymentsRequest = new GetPaymentsRequest();
            getPaymentsRequest.Token = token;
            getPaymentsRequest.DateTo = dateTo.ToString();
            getPaymentsRequest.DateFrom = dateFrom.ToString();
            getPaymentsRequest.CategoryIds = categoryIds.MvcArrayToList();
            getPaymentsRequest.Comment = comment;
            getPaymentsRequest.Place = place;
            var getPaymentsResponse = MoneyWorker.GetPayments(getPaymentsRequest, Request);
            if (getPaymentsResponse.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = getPaymentsResponse.ResponseMessage });
            }
            var model = new PaymentsModel();
            model.Payments = new List<PaymentModel>();
            foreach (var payment in getPaymentsResponse.Body.Payments)
            {
                var p = new PaymentModel();
                p.Id = payment.Id;
                p.CategoryId = payment.CategoryId;
                p.Comment = payment.Comment;
                p.Date = payment.Date.ToDateTime();
                p.Sum = payment.Sum.ToString();
                p.PaymentType = payment.PaymentType;
                model.Payments.Add(p);
            }

            model.Categories = new List<CategoryModel>();
            foreach (var categoryValue in getCategoriesResponse.Body.Categories)
            {
                var c = new CategoryModel();
                c.Id = categoryValue.Id;
                c.Color = categoryValue.Color;
                c.Name = categoryValue.Name;
                c.ParentId = categoryValue.ParentId;
                c.Order = categoryValue.Order;
                c.PaymentTypeId = (int)categoryValue.PaymentType;
                model.Categories.Add(c);
            }

            model.MinDate = dateFrom.ToShortDateString();
            model.MaxDate = dateTo.AddDays(-1).ToShortDateString();
            //return PartialView("Payments", model);



            var workBook = new XLWorkbook();
            var pg = getPaymentsResponse.Body.Payments.GroupBy(x => x.PaymentType);
            foreach (var payments in pg)
            {
                var workSheet = workBook.Worksheets.Add(payments.Key.DescriptionAttr());
                workSheet.Cell("A1").Value = "Id";
                workSheet.Cell("B1").Value = "Id категории";
                workSheet.Cell("C1").Value = "Категория";
                workSheet.Cell("D1").Value = "Дата";
                workSheet.Cell("E1").Value = "Сумма";
                workSheet.Cell("F1").Value = "Комментарий";

                var row = 1;
                foreach (var payment in payments)
                {
                    row++;
                    var column = 1;
                    workSheet.Cell(row, column).Value = payment.Id;
                    column++;
                    workSheet.Cell(row, column).Value = payment.CategoryId;
                    column++;
                    var cat = model.Categories.Single(x => x.Id == payment.CategoryId);
                    workSheet.Cell(row, column).Value = cat.Name;
                    column++;
                    workSheet.Cell(row, column).Value = payment.Date.ToDateTime().ToShortDateString();
                    column++;
                    workSheet.Cell(row, column).Value = payment.Sum.ToString();
                    workSheet.Cell(row, column).Style.NumberFormat.Format = "0.00";
                    column++;
                    workSheet.Cell(row, column).Value = payment.Comment;
                }
            }
            var date = DateTime.Now;
            var directory = Path.GetTempPath();

            string filename = String.Format("BobMoney-{0}{1}{2}{3}{4}{5}{6}.xlsx",
                                            date.Year.ToString("0000"),
                                            date.Month.ToString("00"),
                                            date.Day.ToString("00"),
                                            date.Hour.ToString("00"),
                                            date.Minute.ToString("00"),
                                            date.Second.ToString("00"),
                                            date.Millisecond.ToString("000"));
            var filePath = Path.Combine(directory, filename);
            workBook.SaveAs(filePath);
            return Json(new { success = true, filename = filename });
        }

        public FileResult GetExcelFile(string filename)
        {
            var directory = Path.GetTempPath();
            var filepath = Path.Combine(directory, filename);
            return File(filepath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(filepath));
        }

        [HttpPost]
        public ActionResult GetPaymentsStats(string from, string to, int[] categoryIds = null, string comment = null, string place = null)
        {
            var token = Request.GetAuthToken();
            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = getCategoriesResponse.ResponseMessage });
            }
            var dateFrom = from.ToDateTime();
            var dateTo = to.ToDateTime();
            var getPaymentsRequest = new GetPaymentsRequest();
            getPaymentsRequest.Token = token;
            getPaymentsRequest.DateTo = dateTo.ToString();
            getPaymentsRequest.DateFrom = dateFrom.ToString();
            getPaymentsRequest.CategoryIds = categoryIds.MvcArrayToList();
            getPaymentsRequest.Comment = comment;
            getPaymentsRequest.Place = place;
            var getPaymentsResponse = MoneyWorker.GetPayments(getPaymentsRequest, Request);
            if (getPaymentsResponse.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = getPaymentsResponse.ResponseMessage });
            }
            var model = new PaymentsStatsModel();
            model.Payments = new List<PaymentModel>();
            foreach (var payment in getPaymentsResponse.Body.Payments)
            {
                var p = new PaymentModel();
                p.Id = payment.Id;
                p.CategoryId = payment.CategoryId;
                p.Comment = payment.Comment;
                p.Place = payment.Place;
                p.Date = payment.Date.ToDateTime();
                p.Sum = payment.Sum.ToString();
                p.PaymentType = payment.PaymentType;
                model.Payments.Add(p);
            }
            model.Categories = new List<CategoryModel>();
            foreach (var categoryValue in getCategoriesResponse.Body.Categories)
            {
                var c = new CategoryModel();
                c.Id = categoryValue.Id;
                c.Color = categoryValue.Color;
                c.Name = categoryValue.Name;
                c.Order = categoryValue.Order;
                c.ParentId = categoryValue.ParentId;
                c.PaymentTypeId = (int)categoryValue.PaymentType;
                model.Categories.Add(c);
            }

            model.MinDate = dateFrom.ToShortDateString();
            model.MaxDate = dateTo.AddDays(-1).ToShortDateString();
            return PartialView("PaymentsStats", model);
        }

        [HttpPost]
        public ActionResult GetPaymentForm(string paymentId, string defaultDate, string dayId, int? paymentTypeId)
        {
            var token = Request.GetAuthToken();
            var model = new PaymentFormModel();
            model.DayId = dayId;
            model.Payment = new PaymentModel();
            var id = paymentId.ToInt(null);
            if (id != null)
            {
                var getPaymentRequest = new GetPaymentRequest();
                getPaymentRequest.Token = token;
                getPaymentRequest.PaymentId = (int)id;
                var getPaymentResponse = MoneyWorker.GetPayment(getPaymentRequest, Request);
                if (getPaymentResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = getPaymentResponse.ResponseMessage });
                }
                var gbp = getPaymentResponse.Body.Payment;
                model.Payment.Date = gbp.Date.ToDateTime();
                model.Payment.Id = gbp.Id;
                model.Payment.CategoryId = gbp.CategoryId;
                model.Payment.Sum = gbp.Sum.ToString();
                model.Payment.Comment = gbp.Comment;
                model.Payment.Place = gbp.Place;
                model.Payment.CreatedTaskId = gbp.CreatedTaskId;
                var m2 = GetCategoriesList(Request, (int)gbp.PaymentType);
                if (m2.Categories == null)
                {
                    return PartialView("MyError", new ErrorModel { Message = m2.Message });
                }
                model.Categories = m2.Categories;
                return PartialView("PaymentForm", model);
            }

            var todayInClientTimeZone = DateTime.Now
                .ChangeTimeZoneFromLocalTo(Request.GetClientTimeToUtcShiftInMinutes())
                .Date;
            model.Payment.Date = defaultDate.ToDateTime(todayInClientTimeZone);

            var m = GetCategoriesList(Request, paymentTypeId.Value);
            if (m.Categories == null)
            {
                return PartialView("MyError", new ErrorModel { Message = m.Message });
            }
            model.Categories = m.Categories;
            model.Payment.PaymentType = (PaymentTypes)paymentTypeId.Value;
            return PartialView("PaymentForm", model);
        }

        [HttpPost]
        public ActionResult GetPaymentRow(int paymentId)
        {
            var token = Request.GetAuthToken();

            var getPaymentRequest = new GetPaymentRequest();
            getPaymentRequest.Token = token;
            getPaymentRequest.PaymentId = paymentId;
            var getPaymentResponse = MoneyWorker.GetPayment(getPaymentRequest, Request);
            if (getPaymentResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getPaymentResponse.ResponseMessage });
            }

            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCategoriesResponse.ResponseMessage });
            }

            var payment = getPaymentResponse.Body.Payment;
            var pmModel = new PaymentRowModel();
            var p = new PaymentModel();
            p.Id = payment.Id;
            p.Sum = payment.Sum.ToString();
            p.Comment = payment.Comment;
                p.Place = payment.Place;
            p.CategoryId = payment.CategoryId;
            pmModel.Payment = p;
            if (payment.CategoryId != null)
            {
                pmModel.CategoryName = getCategoriesResponse.Body.Categories.Single(x => x.Id == payment.CategoryId).Name;
            }
            return PartialView("Payment", pmModel);
        }

        [HttpPost]
        public ActionResult GetPaymentDay(int paymentId)
        {
            var token = Request.GetAuthToken();

            var getPaymentRequest = new GetPaymentsRequest();
            getPaymentRequest.Token = token;
            getPaymentRequest.DateFromPayment = paymentId;
            var getPaymentsResponse = MoneyWorker.GetPayments(getPaymentRequest, Request);
            if (getPaymentsResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getPaymentsResponse.ResponseMessage });
            }

            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCategoriesResponse.ResponseMessage });
            }

            var model = new PaymentsDayModel();
            model.Payments = new List<PaymentModel>();
            foreach (var payment in getPaymentsResponse.Body.Payments)
            {
                var p = new PaymentModel();
                p.Id = payment.Id;
                p.CategoryId = payment.CategoryId;
                p.Comment = payment.Comment;
                p.Place = payment.Place;
                p.Date = payment.Date.ToDateTime();
                p.Sum = payment.Sum.ToString();
                p.PaymentType = payment.PaymentType;
                model.Payments.Add(p);
            }

            model.Categories = new List<CategoryModel>();
            foreach (var category in getCategoriesResponse.Body.Categories)
            {
                var c = new CategoryModel();
                c.Id = category.Id;
                c.Color = category.Color;
                c.Name = category.Name;
                c.Order = category.Order;
                c.ParentId = category.ParentId;
                model.Categories.Add(c);
            }
            model.Date = getPaymentsResponse.Body.Payments.First().Date.ToDateTime().Date;
            return PartialView("PaymentsDay", model);
        }

        [HttpPost]
        public ActionResult DeletePayment(int paymentId)
        {
            var token = Request.GetAuthToken();
            var request = new DeletePaymentRequest();
            request.Token = token;
            request.PaymentId = paymentId;
            var response = MoneyWorker.DeletePayment(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult SavePayment(PaymentFormModel paymentForm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("PaymentForm", paymentForm);
            }
            var token = Request.GetAuthToken();
            var payment = paymentForm.Payment;
            decimal sum;
            try
            {
                Expression e = new Expression(payment.Sum.Replace(',', '.'));
                sum = e.Evaluate().ToString().ToDecimal();
            }
            catch (Exception)
            {
                ModelState.AddModelError("Payment.Sum", "Не распознано значение в поле 'сумма'");
                return PartialView("PaymentForm", paymentForm);
            }
            if (payment.Id == null)
            {
                var createPaymentRequest = new CreatePaymentRequest();
                createPaymentRequest.Token = token;
                createPaymentRequest.Sum = sum;
                createPaymentRequest.CategoryId = payment.CategoryId.Value;
                createPaymentRequest.Comment = payment.Comment;
                createPaymentRequest.Place = payment.Place;
                createPaymentRequest.Date = payment.Date.ToString();
                var createResponse = MoneyWorker.CreatePayment(createPaymentRequest, Request);
                if (createResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = createResponse.ResponseMessage });
                }
                payment.Id = createResponse.Body.PaymentId;
            }
            else
            {
                var updatePaymentRequest = new UpdatePaymentRequest();
                updatePaymentRequest.Token = token;
                updatePaymentRequest.PaymentId = payment.Id.Value;
                updatePaymentRequest.Sum = sum;
                updatePaymentRequest.CategoryId = payment.CategoryId.Value;
                updatePaymentRequest.Comment = payment.Comment;
                updatePaymentRequest.Place = payment.Place;
                updatePaymentRequest.Date = payment.Date.ToString();
                var updateResponse = MoneyWorker.UpdatePayment(updatePaymentRequest, Request);
                if (updateResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = updateResponse.ResponseMessage, paymentId = payment.Id });
                }
            }
            var dayId = "day-" + payment.Date.ToString("yyyy-MM-dd");
            return Json(new { success = true, paymentId = payment.Id, dayId = dayId });
        }

        [HttpPost]
        public ActionResult UpdatePayment(int id, string date, string sum, int categoryId, string comment, string place)
        {
            decimal sumVal;
            try
            {
                Expression e = new Expression(sum.Replace(',', '.'));
                sumVal = e.Evaluate().ToString().ToDecimal();
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Не распознано значение в поле 'сумма'" });
            }

            var token = Request.GetAuthToken();
            var updatePaymentRequest = new UpdatePaymentRequest();
            updatePaymentRequest.Token = token;
            updatePaymentRequest.PaymentId = id;
            updatePaymentRequest.Sum = sumVal;
            updatePaymentRequest.CategoryId = categoryId;
            updatePaymentRequest.Comment = comment;
            updatePaymentRequest.Place = place;
            updatePaymentRequest.Date = date;
            var updateResponse = MoneyWorker.UpdatePayment(updatePaymentRequest, Request);
            if (updateResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = updateResponse.ResponseMessage });
            }
            var dayId = "day-" + date.ToDateTime().ToString("yyyy-MM-dd");
            return Json(new { success = true, dayId = dayId, sum = sumVal });
        }

        [HttpPost]
        public ActionResult UpdatePaymentsBatch(int[] ids, int categoryId)
        {
            var token = Request.GetAuthToken();
            var request = new UpdatePaymentsBatchRequest();
            request.Token = token;
            request.PaymentIds = ids.ToList();
            request.CategoryId = categoryId;
            var updateResponse = MoneyWorker.UpdatePaymentsBatch(request, Request);
            if (updateResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = updateResponse.ResponseMessage });
            }

            return Json(new { success = true });
        }

        public ActionResult GetListHelp(string key, string searchString = "")
        {
            var token = Request.GetAuthToken();
            if(key == "place")
            {
                var request = new GetPlacesRequest();
                request.Token = token;
                request.Offset = 0;
                request.Count = 10;
                request.Name = searchString;
                request.SortBy = "startwith,lastuseddate desc";
                var result = MoneyWorker.GetPlaces(request, Request);
                if (result.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = result.ResponseMessage });
                }
                var model = new SearchHelpModel();
                model.Words = new List<SearchHelpModel.HelpKeyWord>();
                foreach (var place in result.Body.Places)
                {
                    var d = new SearchHelpModel.HelpKeyWord();
                    d.Value = place.Name;
                    //d.Text= "kuku";
                    model.Words.Add(d);
                }
                return PartialView("SearchHelp", model);
            }
            return Json(new { success = false, message = "unrecognize key '"+ key +"'" });
        }

        [HttpPost]
        public ActionResult GetDebts(bool withPaid = false)
        {
            var token = Request.GetAuthToken();
            var getDebtsRequest = new GetDebtsRequest();
            getDebtsRequest.Token = token;
            getDebtsRequest.WithPaid = withPaid;
            var response = MoneyWorker.GetDebts(getDebtsRequest, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }
            var model = new DebtsModel();
            model.Debts = new List<DebtModel>();
            foreach (var debt in response.Body.Debts)
            {
                var d = new DebtModel();
                d.Id = debt.Id;
                d.Sum = debt.Sum;
                d.Name = debt.DebtUser.Name;
                d.Status = debt.Status;
                d.Type = debt.Type;
                d.PaySum = debt.PaySum;
                d.PayComment = debt.PayComment;
                d.Date = debt.Date.ToDateTime();
                d.Comment = debt.Comment;
                model.Debts.Add(d);
            }
            return PartialView("Debts", model);
        }


        [HttpPost]
        public ActionResult GetDebtForm()
        {
            var token = Request.GetAuthToken();
            var request = new GetCategoriesRequest();
            request.Token = token;
            var model = new DebtFormModel();
            model.Debt = new DebtModel();
            model.Debt.Date = DateTime.Now.ChangeTimeZoneFromLocalTo(Request.GetClientTimeToUtcShiftInMinutes()).Date;
            model.Types = Enum.GetValues(typeof(DebtTypes)).Cast<DebtTypes>().ToList();
            return PartialView("DebtForm", model);
        }

        [HttpPost]
        public ActionResult SaveDebt(DebtFormModel debtForm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("DebtForm", debtForm);
            }
            var token = Request.GetAuthToken();
            var debt = debtForm.Debt;
            var request = new CreateDebtRequest();
            request.Token = token;
            request.Sum = debt.Sum;
            request.Type = debt.Type;
            request.Date = debt.Date.ToString();
            request.Name = debt.Name;
            request.Comment = debt.Comment;
            var response = MoneyWorker.CreateDebt(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult GetPayDebtForm(int debtId)
        {
            var token = Request.GetAuthToken();
            var request = new GetDebtRequest();
            request.Token = token;
            request.DebtId = debtId;
            var getDebtResponse = MoneyWorker.GetDebt(request, Request);
            if (getDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getDebtResponse.ResponseMessage });
            }
            var debt = getDebtResponse.Body.Debt;
            var model = new PayDebtFormModel();
            model.DebtId = debt.Id;
            model.Date = DateTime.Now.ChangeTimeZoneFromLocalTo(Request.GetClientTimeToUtcShiftInMinutes()).Date;
            model.Sum = debt.Sum - debt.PaySum;

            model.Debt = new PayDebtInfoModel();
            model.Debt.Name = debt.DebtUser.Name;
            model.Debt.Sum = debt.Sum;
            model.Debt.Comment = debt.Comment;
            return PartialView("PayDebtForm", model);
        }


        [HttpPost]
        public ActionResult SavePaymentDebt(PayDebtFormModel debtForm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("PayDebtForm", debtForm);
            }
            var token = Request.GetAuthToken();
            var request = new PayDebtRequest();
            request.Token = token;
            request.DebtId = debtForm.DebtId;
            request.Sum = debtForm.Sum;
            request.Date = debtForm.Date.ToString();
            request.Comment = debtForm.Comment;
            var payDebtResponse = MoneyWorker.PayDebt(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult UpdateDebt(int debtId, decimal sum, string name, string comment, DateTime date)
        {
            var token = Request.GetAuthToken();
            var request = new UpdateDebtRequest();
            request.Token = token;
            request.DebtId = debtId;
            request.Sum = sum;
            request.Name = name;
            request.Comment = comment;
            request.Date = date.ToShortDateString();
            var payDebtResponse = MoneyWorker.UpdateDebt(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult DeleteDebt(int debtId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteDebtRequest();
            request.Token = token;
            request.DebtId = debtId;
            var payDebtResponse = MoneyWorker.DeleteDebt(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult ModeDebtToOperations(int[] debtIds, int categoryId, string comment)
        {
            var debtList = debtIds.ToList();
            var token = Request.GetAuthToken();
            var request = new MoveDebtToOperationsRequest();
            request.Token = token;
            request.DebtIds = debtList;
            request.CategoryId = categoryId;
            request.Comment = comment;
            var payDebtResponse = MoneyWorker.MoveDebtToOperations(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult MergeDebtUsers(int userFrom, int userTo)
        {
            var token = Request.GetAuthToken();
            var request = new MergeDebtUsersRequest();
            request.Token = token;
            request.FromUserId = userFrom;
            request.ToUserId = userTo;
            var payDebtResponse = MoneyWorker.MergeDebtUsers(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult GetCategories(int paymentTypeId)
        {
            var m = GetCategoriesList(Request, paymentTypeId);
            if (m.Categories == null)
            {
                return PartialView("MyError", new ErrorModel { Message = m.Message });
            }
            var model = new CategoriesModel();
            model.Categories = m.Categories;
            return PartialView("Categories", model);
        }

        public class GetCategoriesModel
        {
            public List<CategoryModel> Categories { get; set; }
            public string Message { get; set; }
        }

        public static GetCategoriesModel GetCategoriesList(HttpRequestBase request, int? paymentTypeId)
        {
            var model = new GetCategoriesModel();
            var token = request.GetAuthToken();
            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            getCategoriesRequest.PaymentType = (PaymentTypes?)paymentTypeId;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, request);
            model.Categories = new List<CategoryModel>();
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                model.Message = getCategoriesResponse.ResponseMessage;
                return model;
            }
            foreach (var category in getCategoriesResponse.Body.Categories)
            {
                var c = new CategoryModel();
                c.Id = category.Id;
                c.Color = category.Color;
                c.Name = category.Name;
                c.Order = category.Order;
                c.ParentId = category.ParentId;
                c.PaymentTypeId = (int)category.PaymentType;
                model.Categories.Add(c);
            }
            return model;
        }

        public static List<FastOperationModel> GetFastOperations(HttpRequestBase request)
        {
            var token = request.GetAuthToken();
            var getFastOperationsRequest = new GetFastOperationsRequest();
            getFastOperationsRequest.Token = token;
            var response = MoneyWorker.GetFastOperations(getFastOperationsRequest, request);
            var fastOperations = new List<FastOperationModel>();
            if (response.Type != ResponseType.Success)
            {
                return fastOperations;
            }

            foreach (var fastOperation in response.Body.FastOperations)
            {
                var fastOperationModel = new FastOperationModel();

                fastOperationModel.Id = fastOperation.Id;
                fastOperationModel.PaymentType = fastOperation.PaymentType;
                fastOperationModel.Name = fastOperation.Name;
                fastOperationModel.Order = fastOperation.Order;
                fastOperationModel.Sum = fastOperation.Sum;
                fastOperationModel.CategoryId = fastOperation.CategoryId;
                fastOperationModel.Comment = fastOperation.Comment;
                fastOperationModel.Place = fastOperation.Place;

                fastOperations.Add(fastOperationModel);
            }
            return fastOperations;
        }

        [HttpPost]
        public ActionResult GetCategoryForm(string categoryId, int? paymentTypeId = null)
        {
            var token = Request.GetAuthToken();
            var getCategoriesRequest = new GetCategoriesRequest();
            getCategoriesRequest.Token = token;
            getCategoriesRequest.PaymentType = (PaymentTypes?)paymentTypeId;
            var getCategoriesResponse = MoneyWorker.GetCategories(getCategoriesRequest, Request);
            if (getCategoriesResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCategoriesResponse.ResponseMessage });
            }
            var model = new CategoryFormModel();
            model.Categories = new List<CategoryModel>();
            foreach (var category in getCategoriesResponse.Body.Categories)
            {
                var c = new CategoryModel();
                c.Id = category.Id;
                c.Color = category.Color;
                c.Name = category.Name;
                c.Order = category.Order;
                c.ParentId = category.ParentId;
                model.Categories.Add(c);
            }
            var id = categoryId.ToInt(null);
            if (id != null)
            {
                var getPaymentRequest = new GetCategoryRequest();
                getPaymentRequest.Token = token;
                getPaymentRequest.CategoryId = (int)id;
                var getPaymentResponse = MoneyWorker.GetCategory(getPaymentRequest, Request);
                if (getPaymentResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = getPaymentResponse.ResponseMessage });
                }
                model.Category = new CategoryModel();
                model.Category.Id = getPaymentResponse.Body.Category.Id;
                model.Category.Name = getPaymentResponse.Body.Category.Name;
                model.Category.Order = getPaymentResponse.Body.Category.Order;
                model.Category.Color = getPaymentResponse.Body.Category.Color;
                model.Category.PaymentTypeId = (int)getPaymentResponse.Body.Category.PaymentType;
                model.Category.ParentId = getPaymentResponse.Body.Category.ParentId;
                return PartialView("CategoryForm", model);
            }
            model.Category = new CategoryModel();
            model.Category.PaymentTypeId = paymentTypeId.Value;
            return PartialView("CategoryForm", model);
        }

        [HttpPost]
        public ActionResult SaveCategory(CategoryFormModel form)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("CategoryForm", form);
            }
            var token = Request.GetAuthToken();
            var category = form.Category;
            if (category.Id == null)
            {
                var request = new CreateCategoryRequest();
                request.Token = token;
                request.ParentId = category.ParentId == -1 ? null : category.ParentId;
                request.Color = category.Color;
                request.Name = category.Name;
                request.PaymentType = (PaymentTypes)category.PaymentTypeId;
                request.Order = category.Order;
                var response = MoneyWorker.CreateCategory(request, Request);
                if (response.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = response.ResponseMessage });
                }
            }
            else
            {
                var request = new UpdateCategoryRequest();
                request.CategoryId = category.Id.Value;
                request.Token = token;
                request.ParentId = category.ParentId == -1 ? null : category.ParentId;
                request.Color = category.Color;
                request.Name = category.Name;
                request.Order = category.Order;
                var response = MoneyWorker.UpdateCategory(request, Request);
                if (response.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = response.ResponseMessage });
                }
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult UpdateCategory(int categoryId, string name, string color, int? parentId, int? order)
        {
            var token = Request.GetAuthToken();
            var request = new UpdateCategoryRequest();
            request.Token = token;
            request.CategoryId = categoryId;
            request.ParentId = parentId;
            request.Name = name;
            request.Order = order;
            request.Color = color;
            var payDebtResponse = MoneyWorker.UpdateCategory(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult DeleteCategory(int categoryId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteCategoryRequest();
            request.Token = token;
            request.CategoryId = categoryId;
            var response = MoneyWorker.DeleteCategory(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }



        [HttpPost]
        public ActionResult GetRegularTasks()
        {
            var token = Request.GetAuthToken();
            var getRegularTasksRequest = new GetRegularTasksRequest();
            getRegularTasksRequest.Token = token;
            var response = MoneyWorker.GetRegularTasks(getRegularTasksRequest, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }
            var model = new RegularTasksModel();
            model.RegularTasks = new List<RegularTaskModel>();
            foreach (var regularTask in response.Body.Tasks)
            {
                var task = new RegularTaskModel();
                task.Id = regularTask.Id;
                task.Type = regularTask.Type;
                task.Name = regularTask.Name;
                task.TimeType = regularTask.TimeType;
                task.TimeValue = regularTask.TimeValue;
                task.DateFrom = regularTask.DateFrom.ToDateTime();
                task.DateTo = regularTask.DateTo.ToDateTime();
                task.RunTime = regularTask.RunTime.ToDateTime();

                if (regularTask.Payment != null)
                {
                    task.Payment = new RegularTaskModel.PaymentModel();
                    task.Payment.CategoryId = regularTask.Payment.CategoryId.Value;
                    task.Payment.PaymentType = regularTask.Payment.PaymentType;
                    task.Payment.Place = regularTask.Payment.Place;
                    task.Payment.Comment = regularTask.Payment.Comment;
                    task.Payment.Sum = regularTask.Payment.Sum;
                }

                model.RegularTasks.Add(task);
            }
            return PartialView("RegularTasks", model);
        }


        [HttpPost]
        public ActionResult GetRegularTaskForm(int? regularTaskId = null)
        {
            var token = Request.GetAuthToken();
            var request = new GetCategoriesRequest();
            request.Token = token;
            var m = GetCategoriesList(Request, null);
            if (m.Categories == null)
            {
                return Json(new { success = false, message = m.Message });
            }
            var model = new RegularTaskFormModel();
            model.PaymentCategories = m.Categories;
            model.Types = Enum.GetValues(typeof(RegularTaskTypes)).Cast<RegularTaskTypes>().ToList();
            model.TimeTypes = Enum.GetValues(typeof(RegularTaskTimeTypes)).Cast<RegularTaskTimeTypes>().ToList();

            if (regularTaskId != null)
            {
                var getRegularTaskRequest = new GetRegularTaskRequest();
                getRegularTaskRequest.Token = token;
                getRegularTaskRequest.RegularTaskId = regularTaskId.Value;
                var response = MoneyWorker.GetRegularTask(getRegularTaskRequest, Request);
                if (response.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = m.Message });
                }
                var regularTask = response.Body.Task;

                var task = new RegularTaskModel();
                task.Id = regularTask.Id;
                task.Type = regularTask.Type;
                task.Name = regularTask.Name;
                task.TimeType = regularTask.TimeType;
                if (regularTask.TimeType == RegularTaskTimeTypes.EveryWeek)
                {
                    task.TimeValue2 = regularTask.TimeValue;
                }
                else if (regularTask.TimeType == RegularTaskTimeTypes.EveryMonth)
                {
                    task.TimeValue3 = regularTask.TimeValue;

                }
                task.DateFrom = regularTask.DateFrom.ToDateTime();
                task.DateTo = regularTask.DateTo.ToDateTime();

                if (regularTask.Payment != null)
                {
                    task.Payment = new RegularTaskModel.PaymentModel();
                    task.Payment.CategoryId = regularTask.Payment.CategoryId.Value;
                    task.Payment.PaymentType = regularTask.Payment.PaymentType;
                    task.Payment.Place = regularTask.Payment.Place;
                    task.Payment.Comment = regularTask.Payment.Comment;
                    task.Payment.Sum = regularTask.Payment.Sum;
                }

                model.RegularTask = task;
            }
            else
            {
                model.RegularTask = new RegularTaskModel();
                model.RegularTask.DateFrom = DateTime.Now;
            }

            return PartialView("RegularTaskForm", model);
        }

        [HttpPost]
        public ActionResult SaveRegularTask(RegularTaskFormModel form)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("RegularTaskForm", form);
            }

            var token = Request.GetAuthToken();
            var regularTask = form.RegularTask;
            if (regularTask.Id == null)
            {
                var createRegularTaskRequest = new CreateRegularTaskRequest();
                createRegularTaskRequest.Token = token;
                createRegularTaskRequest.DateFrom = regularTask.DateFrom.ToShortDateString();
                createRegularTaskRequest.DateTo = regularTask.DateTo?.ToShortDateString();
                createRegularTaskRequest.Name = regularTask.Name;
                createRegularTaskRequest.TimeType = regularTask.TimeType;
                if (regularTask.TimeType == RegularTaskTimeTypes.EveryMonth)
                {
                    if (regularTask.TimeValue3 == null)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue3", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    if (regularTask.TimeValue3 < 1 || regularTask.TimeValue3 > 31)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue3", "От 1 до 31");
                        return PartialView("RegularTaskForm", form);
                    }
                    createRegularTaskRequest.TimeValue = regularTask.TimeValue3.Value;

                }
                else if (regularTask.TimeType == RegularTaskTimeTypes.EveryWeek)
                {
                    if (regularTask.TimeValue2 == null)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue2", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    if (regularTask.TimeValue2 < 1 || regularTask.TimeValue2 > 7)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue2", "От 1 до 7");
                        return PartialView("RegularTaskForm", form);
                    }
                    createRegularTaskRequest.TimeValue = regularTask.TimeValue2.Value;

                }

                createRegularTaskRequest.Type = regularTask.Type;
                if (regularTask.Type == RegularTaskTypes.Operation)
                {
                    createRegularTaskRequest.Payment = new CreateRegularTaskRequest.PaymentValue();
                    if (regularTask.Payment.CategoryId == null)
                    {
                        ModelState.AddModelError("RegularTask.Payment.CategoryId", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    createRegularTaskRequest.Payment.CategoryId = regularTask.Payment.CategoryId.Value;
                    if (regularTask.Payment.Sum == null)
                    {
                        ModelState.AddModelError("RegularTask.Payment.Sum", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    createRegularTaskRequest.Payment.Sum = regularTask.Payment.Sum.Value;
                    createRegularTaskRequest.Payment.Place = regularTask.Payment.Place;
                    createRegularTaskRequest.Payment.Comment = regularTask.Payment.Comment;
                }
                var createResponse = MoneyWorker.CreateRegularTask(createRegularTaskRequest, Request);
                if (createResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = createResponse.ResponseMessage });
                }
                regularTask.Id = createResponse.Body.RegularTaskId;
            }
            else
            {
                //задублирую как бох)
                var updateRegularTaskRequest = new UpdateRegularTaskRequest();
                updateRegularTaskRequest.Token = token;
                updateRegularTaskRequest.Id = regularTask.Id.Value;
                updateRegularTaskRequest.DateFrom = regularTask.DateFrom.ToShortDateString();
                updateRegularTaskRequest.DateTo = regularTask.DateTo?.ToShortDateString();
                updateRegularTaskRequest.Name = regularTask.Name;
                updateRegularTaskRequest.TimeType = regularTask.TimeType;
                if (regularTask.TimeType == RegularTaskTimeTypes.EveryMonth)
                {
                    if (regularTask.TimeValue3 == null)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue3", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    if (regularTask.TimeValue3 < 1 || regularTask.TimeValue3 > 31)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue3", "От 1 до 31");
                        return PartialView("RegularTaskForm", form);
                    }
                    updateRegularTaskRequest.TimeValue = regularTask.TimeValue3.Value;

                }
                else if (regularTask.TimeType == RegularTaskTimeTypes.EveryWeek)
                {
                    if (regularTask.TimeValue2 == null)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue2", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    if (regularTask.TimeValue2 < 1 || regularTask.TimeValue2 > 7)
                    {
                        ModelState.AddModelError("RegularTask.TimeValue2", "От 1 до 7");
                        return PartialView("RegularTaskForm", form);
                    }
                    updateRegularTaskRequest.TimeValue = regularTask.TimeValue2.Value;
                }

                updateRegularTaskRequest.Type = regularTask.Type;
                if (regularTask.Type == RegularTaskTypes.Operation)
                {
                    updateRegularTaskRequest.Payment = new UpdateRegularTaskRequest.PaymentValue();
                    if (regularTask.Payment.CategoryId == null)
                    {
                        ModelState.AddModelError("RegularTask.Payment.CategoryId", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    updateRegularTaskRequest.Payment.CategoryId = regularTask.Payment.CategoryId.Value;
                    if (regularTask.Payment.Sum == null)
                    {
                        ModelState.AddModelError("RegularTask.Payment.Sum", "Не распознано значение");
                        return PartialView("RegularTaskForm", form);
                    }
                    updateRegularTaskRequest.Payment.Sum = regularTask.Payment.Sum.Value;
                    updateRegularTaskRequest.Payment.Place = regularTask.Payment.Place;
                    updateRegularTaskRequest.Payment.Comment = regularTask.Payment.Comment;
                }
                var createResponse = MoneyWorker.UpdateRegularTask(updateRegularTaskRequest, Request);
                if (createResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = createResponse.ResponseMessage });
                }
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult DeleteRegularTask(int regularTaskId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteRegularTaskRequest();
            request.Token = token;
            request.RegularTaskId = regularTaskId;
            var payDebtResponse = MoneyWorker.DeleteRegularTask(request, Request);
            if (payDebtResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = payDebtResponse.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }
    }
}