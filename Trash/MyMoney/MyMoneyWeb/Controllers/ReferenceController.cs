using Extentions;
using MyMoneyWeb.Models.UmsPicker;
using MyMoneyWeb.Models;
using MyMoneyWeb.Structure;
using ServiceResponse;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace MyMoneyWeb.Controllers
{
    public class ReferenceController : BaseController
    {

        public ActionResult GetDropdownPickerValues(string pickerId, string searchString, string searchTemplate,
            string orderTemplate, string showColumnNames, string offset, string count, string pickerType, string idColumnName, string additionalValues,
            string selectAllText, int typeId = 0, bool disableLoadNextButton = false, bool isHideSearch = false)
        {
            var token = Request.GetAuthToken();
            var offsetAsInt = offset.ToInt(0);
            var countAsInt = count.ToInt(10);
            ////string search = null;
            ////if (!String.IsNullOrEmpty(searchTemplate))
            ////{
            ////    search = searchTemplate.Replace("{param1}", searchString);
            ////}
            ////string order = null;
            ////if (!String.IsNullOrEmpty(orderTemplate))
            ////{
            ////    if (!String.IsNullOrEmpty(searchString))
            ////    {
            ////        order = orderTemplate.Replace("{param1}", searchString);
            ////    }
            ////    else
            ////    {
            ////        var orderParts = orderTemplate.Split(';');
            ////        foreach (var orderPart in orderParts.Where(x => !x.Contains("{param1}")))
            ////        {
            ////            order += orderPart + ";";
            ////        }
            ////    }
            ////}

            ////var getByNameRequest = new GetByNameRequest();
            ////getByNameRequest.Token = token;
            ////getByNameRequest.Name = pickerType;
            ////getByNameRequest.SearchString = search;
            ////getByNameRequest.OrderString = order;
            ////getByNameRequest.Offset = offsetAsInt;
            ////getByNameRequest.Count = countAsInt;
            ////getByNameRequest.TypeId = typeId;
            ////var result = ReferenceWorker.GetByName(getByNameRequest, Request);
            ////if (result.Info.Type != Common.Core.Services.RequestStatusTypes.Success)
            ////{
            ////    return Json(new { success = false, message = result.Info.Message });
            ////}

            var values = new List<ReferenceValue>();
            var totalCount = 0;
            var searchCount = 0;
            //можно в константы вынести
            if (pickerType == "MyOperationCategoryList" || pickerType == "MyOperationCategoryMinusList")
            {
                var getCategoriesRequest = new ServiceRequest.Money.GetCategoriesRequest();
                getCategoriesRequest.Token = token;
                var getCategoriesResponse = ServiceWorker.MoneyWorker.GetCategories(getCategoriesRequest, Request);
                if (getCategoriesResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = getCategoriesResponse.ResponseMessage });
                }

                var categories = new List<CategoryModel>();
                var total = 0;
                foreach (var categoryValue in getCategoriesResponse.Body.Categories)
                {
                    if (pickerType == "MyOperationCategoryMinusList")
                    {
                        if (categoryValue.PaymentType == Common.Enums.PaymentTypes.Income)
                        {
                            continue;
                        }
                    }

                    var c = new CategoryModel();
                    c.Id = categoryValue.Id;
                    c.Color = categoryValue.Color;
                    c.Name = categoryValue.Name;
                    c.Order = categoryValue.Order;
                    c.ParentId = categoryValue.ParentId;
                    c.PaymentTypeId = (int)categoryValue.PaymentType;
                    categories.Add(c);
                }

                foreach (var category in categories.SortByParent())
                {
                    // todo можно просто на фронте фильтровать :) но так вышло
                    if (string.IsNullOrEmpty(searchString) || (category.Name ?? "").ToLower().Contains(searchString.ToLower()))
                    {
                        var val = new ReferenceValue();
                        val.Id = category.Id.ToString();
                        val.Values = new List<KeyValue>();
                        val.Values.Add(new KeyValue { Key = "Name", Value = category.Name });
                        values.Add(val);
                    }
                    total++;
                }
                totalCount = total;
                searchCount = values.Count;
            }
            if (pickerType == "MyDebtUserList")
            {
                var getDebtUsersReqeust = new ServiceRequest.Money.GetDebtUsersRequest();
                getDebtUsersReqeust.Token = token;
                getDebtUsersReqeust.Search = searchString;
                var getDebtUsersResponse = ServiceWorker.MoneyWorker.GetDebtUsers(getDebtUsersReqeust, Request);
                if (getDebtUsersResponse.Type != ResponseType.Success)
                {
                    return Json(new { success = false, message = getDebtUsersResponse.ResponseMessage });
                }

                foreach (var debtUser in getDebtUsersResponse.Body.DebtUsers)
                {
                    var val = new ReferenceValue();
                    val.Id = debtUser.Id.ToString();
                    val.Values = new List<KeyValue>();
                    val.Values.Add(new KeyValue { Key = "Name", Value = debtUser.Name });
                    values.Add(val);
                }
                totalCount = getDebtUsersResponse.Body.TotalCount;
                searchCount = values.Count;
            }

            var model = new DropDownPickerValuesModel();
            model.Values = values;
            model.CurrentOffset = offsetAsInt;
            model.SearchString = searchString;
            model.TotalCount = totalCount;
            model.SearchCount = searchCount;
            model.ShowColumns = showColumnNames;
            model.Id = pickerId;
            model.IdColumnName = idColumnName;
            model.SelectAllText = selectAllText;
            model.AdditionalValues = additionalValues;
            model.DisableLoadNextButton = disableLoadNextButton;
            model.IsHideSearch = isHideSearch;
            return PartialView("UmsPicker/DropDownPickerValues", model);
        }
    }
}
