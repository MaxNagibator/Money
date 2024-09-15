using Common;
using Common.Service;
using DataWorker;
using Extentions;
using ServiceRequest.Money;
using ServiceRespone.Money;
using ServiceResponse;
using ServiceWorker.Executor;
using System;
using System.Collections.Generic;
using System.Web;

namespace ServiceWorker
{
    public class MoneyWorker
    {
        public static Response<CreateCategoryResponse> CreateCategory(CreateCategoryRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateCategoryResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new CreateCategoryResponse();
                    result.CategoryId = DbFinanceWorker.CreateCategory(userId, request.Name, request.Description, request.ParentId, request.Color, request.PaymentType, request.Order);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.CreateCategory);
        }

        public static Response<UpdateCategoryResponse> UpdateCategory(UpdateCategoryRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdateCategoryResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdateCategoryResponse();
                    DbFinanceWorker.UpdateCategory(userId, request.CategoryId, request.Name, request.Description, request.ParentId, request.Color, request.Order);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.UpdateCategory);
        }

        public static Response<DeleteCategoryResponse> DeleteCategory(DeleteCategoryRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteCategoryResponse> x =
               () =>
               {
                   var userId = request.GetUserIdByToken();
                   var result = new DeleteCategoryResponse();
                   DbFinanceWorker.DeleteCategory(userId, request.CategoryId);
                   return result;
               };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.DeleteCategory);
        }

        public static Response<GetCategoryResponse> GetCategory(GetCategoryRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetCategoryResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetCategoryResponse();
                    var category = DbFinanceWorker.GetCategory(userId, request.CategoryId);
                    result.Category = new GetCategoryResponse.CategoryValue();
                    var c = new GetCategoryResponse.CategoryValue();
                    c.Id = category.Id;
                    c.Color = category.Color;
                    c.Name = category.Name;
                    c.ParentId = category.ParentId;
                    c.PaymentType = category.PaymentType;
                    c.Order = category.Order;
                    result.Category = c;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetCategory);
        }

        public static Response<GetCategoriesResponse> GetCategories(GetCategoriesRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetCategoriesResponse> x =
                () =>
                {
                    var result = new GetCategoriesResponse();
                    var userId = request.GetUserIdByToken();
                    var catergories = DbFinanceWorker.GetCategories(userId, request.PaymentType);
                    result.Categories = new List<GetCategoriesResponse.CategoryValue>();
                    foreach (var category in catergories)
                    {
                        var c = new GetCategoriesResponse.CategoryValue();
                        c.Id = category.Id;
                        c.Color = category.Color;
                        c.Name = category.Name;
                        c.ParentId = category.ParentId;
                        c.PaymentType = category.PaymentType;
                        c.Order = category.Order;
                        result.Categories.Add(c);
                    }
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetCategories);
        }

        public static Response<CreatePaymentResponse> CreatePayment(CreatePaymentRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreatePaymentResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var payDate = String.IsNullOrEmpty(request.Date) ? DateTime.Now.Date : request.Date.ToDateTime();
                    var result = new CreatePaymentResponse();
                    result.PaymentId = DbFinanceWorker.CreatePayment(userId, request.Sum, request.CategoryId, request.Comment, payDate, request.Place);
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("CategoryId", request.CategoryId));
            parameters.Add(new ServiceParam("Date", request.Date));
            parameters.Add(new ServiceParam("Sum", request.Sum));
            parameters.Add(new ServiceParam("Comment", request.Comment));
            parameters.Add(new ServiceParam("Place", request.Place));
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.CreatePayment, parameters);
        }

        public static Response<UpdatePaymentResponse> UpdatePayment(UpdatePaymentRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdatePaymentResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var payDate = String.IsNullOrEmpty(request.Date) ? DateTime.Now.Date : request.Date.ToDateTime();
                    var result = new UpdatePaymentResponse();
                    DbFinanceWorker.UpdatePayment(userId, request.PaymentId, request.Sum, request.CategoryId, request.Comment, payDate, request.Place);
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("PaymentId", request.PaymentId));
            parameters.Add(new ServiceParam("CategoryId", request.CategoryId));
            parameters.Add(new ServiceParam("Date", request.Date));
            parameters.Add(new ServiceParam("Sum", request.Sum));
            parameters.Add(new ServiceParam("Comment", request.Comment));
            parameters.Add(new ServiceParam("Place", request.Place));
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.UpdatePayment, parameters);
        }

        public static Response<UpdatePaymentsBatchResponse> UpdatePaymentsBatch(UpdatePaymentsBatchRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdatePaymentsBatchResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdatePaymentsBatchResponse();
                    DbFinanceWorker.UpdatePaymentsBatch(userId, request.PaymentIds, request.CategoryId);
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("PaymentId", request.PaymentIds.Aggregate(",")));
            parameters.Add(new ServiceParam("CategoryId", request.CategoryId));
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.UpdatePaymentsBatch, parameters);
        }

        public static Response<DeletePaymentResponse> DeletePayment(DeletePaymentRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeletePaymentResponse> x =
               () =>
               {
                   var userId = request.GetUserIdByToken();
                   var result = new DeletePaymentResponse();
                   DbFinanceWorker.DeletePayment(userId, request.PaymentId);
                   return result;
               };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.DeletePayment);

        }

        public static Response<GetPaymentResponse> GetPayment(GetPaymentRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetPaymentResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetPaymentResponse();
                    var payment = DbFinanceWorker.GetPayment(userId, request.PaymentId);
                    var p = new GetPaymentResponse.PaymentValue();
                    p.Id = payment.Id;
                    p.Sum = payment.Sum;
                    p.Comment = payment.Comment;
                    p.CreatedTaskId = payment.CreatedTaskId;
                    p.CategoryId = payment.CategoryId;
                    p.Date = payment.Date.ToUnixDate();
                    p.PaymentType = payment.PaymentType;
                    p.Place = payment.Place;
                    result.Payment = p;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetPayment);
        }

        public static Response<GetPaymentsResponse> GetPayments(GetPaymentsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetPaymentsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetPaymentsResponse();
                    var from = request.DateFrom.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                    var to = request.DateTo.ToDateTime(from.AddMonths(1));
                    var dateFromPaymentId = request.DateFromPayment;
                    var category = request.CategoryIds;
                    var comment = request.Comment;
                    var place = request.Place;
                    var payments = DbFinanceWorker.GetPayments(userId, from, to, category, comment, place, dateFromPaymentId);
                    result.Payments = new List<GetPaymentsResponse.PaymentValue>();
                    foreach (var payment in payments)
                    {
                        var p = new GetPaymentsResponse.PaymentValue();
                        p.Id = payment.Id;
                        p.CategoryId = payment.CategoryId;
                        p.Comment = payment.Comment;
                        p.Date = payment.Date.ToUnixDate();
                        p.Sum = payment.Sum;
                        p.Place = payment.Place;
                        p.PaymentType = payment.PaymentType;
                        p.CreatedTaskId = payment.CreatedTaskId;
                        result.Payments.Add(p);
                    }
                    return result;
                };
            var parameters = new List<ServiceParam>();
            parameters.Add(new ServiceParam("DateFrom", request.DateFrom));
            parameters.Add(new ServiceParam("DateTo", request.DateTo));
            parameters.Add(new ServiceParam("CategoryIds", request.CategoryIds.Aggregate()));
            parameters.Add(new ServiceParam("Comment", request.Comment));
            parameters.Add(new ServiceParam("Place", request.Place));
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetPayments, parameters);
        }

        public static Response<GetPaymentStatisticsResponse> GetPaymentStatistics(GetPaymentStatisticsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetPaymentStatisticsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetPaymentStatisticsResponse();
                    var from = request.DateFrom.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                    var to = request.DateTo.ToDateTime(from.AddMonths(1));
                    //result.Statistics = DbFinanceWorker.GetPaymentStatisticsResponse(userId, from, to);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetPaymentStatistics);
        }

        public static Response<GetPlacesResponse> GetPlaces(GetPlacesRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetPlacesResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetPlacesResponse();
                    var places = DbFinanceWorker.GetPlaces(userId, request.Name, request.Offset, request.Count, request.SortBy);
                    result.Places = new List<GetPlacesResponse.PlaceValue>();
                    foreach (var debt in places)
                    {
                        var d = new GetPlacesResponse.PlaceValue();
                        d.Id = debt.Id;
                        d.Name = debt.Name;
                        result.Places.Add(d);
                    }
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetPlaces);
        }

        public static Response<GetDebtsResponse> GetDebts(GetDebtsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetDebtsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetDebtsResponse();
                    var debts = DbFinanceWorker.GetDebts(userId, request.WithPaid);
                    result.Debts = new List<GetDebtsResponse.DebtValue>();
                    foreach (var debt in debts)
                    {
                        var d = new GetDebtsResponse.DebtValue();
                        d.Id = debt.Id;
                        d.Sum = debt.Sum;
                        d.Status = debt.Status;
                        d.Type = debt.Type;
                        d.PaySum = debt.PaySum;
                        d.PayComment = debt.PayComment;
                        d.Date = debt.Date.ToUnixDate();
                        d.Comment = debt.Comment;

                        var du = new GetDebtsResponse.DebtUserValue();
                        du.Id = debt.DebtUser.Id;
                        du.Name = debt.DebtUser.Name;
                        d.DebtUser = du;

                        result.Debts.Add(d);
                    }
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetDebts);
        }

        public static Response<GetDebtResponse> GetDebt(GetDebtRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetDebtResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetDebtResponse();
                    var debt = DbFinanceWorker.GetDebt(userId, request.DebtId);

                    var d = new GetDebtResponse.DebtValue();
                    d.Id = debt.Id;
                    d.Sum = debt.Sum;
                    d.Status = debt.Status;
                    d.Type = debt.Type;
                    d.PaySum = debt.PaySum;
                    d.PayComment = debt.PayComment;
                    d.Date = debt.Date.ToUnixDate();
                    d.Comment = debt.Comment;

                    var du = new GetDebtResponse.DebtUserValue();
                    du.Id = debt.DebtUser.Id;
                    du.Name = debt.DebtUser.Name;
                    d.DebtUser = du;

                    result.Debt = d;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetDebt);
        }

        public static Response<CreateDebtResponse> CreateDebt(CreateDebtRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateDebtResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var payDate = String.IsNullOrEmpty(request.Date) ? DateTime.Now.Date : request.Date.ToDateTime();
                    var result = new CreateDebtResponse();
                    result.DebtId = DbFinanceWorker.CreateDebt(userId, request.Sum, request.Type, payDate, request.Name, request.Comment);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.CreateDebt);
        }

        public static Response<PayDebtResponse> PayDebt(PayDebtRequest request, HttpRequestBase httpRequestBase)
        {
            Func<PayDebtResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var payDate = String.IsNullOrEmpty(request.Date) ? DateTime.Now.Date : request.Date.ToDateTime();
                    var result = new PayDebtResponse();
                    DbFinanceWorker.PayDebt(userId, request.DebtId, request.Sum, payDate, request.Comment);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.PayDebt);
        }

        public static Response<UpdateDebtResponse> UpdateDebt(UpdateDebtRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdateDebtResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdateDebtResponse();
                    DbFinanceWorker.UpdateDebt(userId, request.DebtId, request.Sum, request.Name, request.Comment, request.Date.ToDateTime());
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.UpdateDebt);
        }

        public static Response<DeleteDebtResponse> DeleteDebt(DeleteDebtRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteDebtResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new DeleteDebtResponse();
                    DbFinanceWorker.DeleteDebt(userId, request.DebtId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.DeleteDebt);
        }

        public static Response<MoveDebtToOperationsResponse> MoveDebtToOperations(MoveDebtToOperationsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<MoveDebtToOperationsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new MoveDebtToOperationsResponse();
                    DbFinanceWorker.MoveDebtToOperations(userId, request.DebtIds, request.CategoryId, request.Comment);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.MoveDebtToOperations);
        }

        public static Response<GetDebtUsersResponse> GetDebtUsers(GetDebtUsersRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetDebtUsersResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetDebtUsersResponse();
                    int totalCount;
                    var users = DbFinanceWorker.GetDebtUsers(userId, request.Search, out totalCount);

                    var debtUsers = new List<GetDebtUsersResponse.DebtUserValue>();
                    foreach (var user in users)
                    {
                        var du = new GetDebtUsersResponse.DebtUserValue();
                        du.Id = user.Id;
                        du.Name = user.Name;
                        debtUsers.Add(du);
                    }
                    result.DebtUsers = debtUsers;
                    result.TotalCount = totalCount;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetDebtUsers);
        }

        public static Response<MergeDebtUsersResponse> MergeDebtUsers(MergeDebtUsersRequest request, HttpRequestBase httpRequestBase)
        {
            Func<MergeDebtUsersResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new MergeDebtUsersResponse();
                    DbFinanceWorker.MergeDebtUsers(userId, request.FromUserId, request.ToUserId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.MergeDebtUsers);
        }

        public static Response<GetRegularTasksResponse> GetRegularTasks(GetRegularTasksRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetRegularTasksResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetRegularTasksResponse();
                    var regularTasks = DbFinanceWorker.GetRegularTasks(userId);
                    result.Tasks = new List<GetRegularTasksResponse.TaskValue>();
                    foreach (var regularTask in regularTasks)
                    {
                        var task = new GetRegularTasksResponse.TaskValue();
                        task.Id = regularTask.Id;
                        task.Type = regularTask.Type;
                        task.Name = regularTask.Name;
                        task.TimeType = regularTask.TimeType;
                        task.TimeValue = regularTask.TimeValue;
                        task.DateFrom = regularTask.DateFrom.ToUnixDate();
                        task.DateTo = regularTask.DateTo.ToUnixDate();
                        task.RunTime = regularTask.RunTime.ToUnixDate();
                        if (regularTask.Payment != null)
                        {
                            task.Payment = new GetRegularTasksResponse.TaskValue.PaymentValue();
                            task.Payment.CategoryId = regularTask.Payment.CategoryId;
                            task.Payment.PaymentType = regularTask.Payment.PaymentType;
                            task.Payment.Place = regularTask.Payment.Place;
                            task.Payment.Comment = regularTask.Payment.Comment;
                            task.Payment.Sum = regularTask.Payment.Sum;
                        }
                        result.Tasks.Add(task);
                    }

                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetRegularTasks);
        }

        public static Response<GetRegularTaskResponse> GetRegularTask(GetRegularTaskRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetRegularTaskResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetRegularTaskResponse();
                    var regularTask = DbFinanceWorker.GetRegularTask(userId, request.RegularTaskId);

                    var task = new GetRegularTaskResponse.TaskValue();
                    task.Id = regularTask.Id;
                    task.Type = regularTask.Type;
                    task.Name = regularTask.Name;
                    task.TimeType = regularTask.TimeType;
                    task.TimeValue = regularTask.TimeValue;
                    task.DateFrom = regularTask.DateFrom.ToUnixDate();
                    task.DateTo = regularTask.DateTo.ToUnixDate();
                    task.RunTime = regularTask.RunTime.ToUnixDate();
                    if (regularTask.Payment != null)
                    {
                        task.Payment = new GetRegularTaskResponse.TaskValue.PaymentValue();
                        task.Payment.CategoryId = regularTask.Payment.CategoryId;
                        task.Payment.PaymentType = regularTask.Payment.PaymentType;
                        task.Payment.Place = regularTask.Payment.Place;
                        task.Payment.Comment = regularTask.Payment.Comment;
                        task.Payment.Sum = regularTask.Payment.Sum;
                    }

                    result.Task = task;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetRegularTask);
        }

        public static Response<CreateRegularTaskResponse> CreateRegularTask(CreateRegularTaskRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateRegularTaskResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new CreateRegularTaskResponse();
                    var dateFrom = request.DateFrom.ToDateTime();
                    var dateTo = request.DateTo?.ToDateTime();

                    Payment payment = null;
                    if (request.Payment != null)
                    {
                        payment = new Payment();
                        payment.CategoryId = request.Payment.CategoryId;
                        payment.Sum = request.Payment.Sum;
                        payment.Place = request.Payment.Place;
                        payment.Comment = request.Payment.Comment;
                    }

                    result.RegularTaskId = DbFinanceWorker.CreateReqularTask(userId, request.Name, request.Type, request.TimeType, request.TimeValue, dateFrom, dateTo, payment);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.CreateRegularTask);
        }

        public static Response<UpdateRegularTaskResponse> UpdateRegularTask(UpdateRegularTaskRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdateRegularTaskResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdateRegularTaskResponse();
                    var dateFrom = request.DateFrom.ToDateTime();
                    var dateTo = request.DateTo?.ToDateTime();

                    Payment payment = null;
                    if (request.Payment != null)
                    {
                        payment = new Payment();
                        payment.CategoryId = request.Payment.CategoryId;
                        payment.Sum = request.Payment.Sum;
                        payment.Place = request.Payment.Place;
                        payment.Comment = request.Payment.Comment;
                    }

                    DbFinanceWorker.UpdateReqularTask(userId, request.Id, request.Name, request.TimeType, request.TimeValue, dateFrom, dateTo, payment);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.UpdateRegularTask);
        }

        public static Response<DeleteRegularTaskResponse> DeleteRegularTask(DeleteRegularTaskRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteRegularTaskResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new DeleteRegularTaskResponse();
                    DbFinanceWorker.DeleteReqularTask(userId, request.RegularTaskId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.DeleteRegularTask);
        }

        public static Response<RunRegularTaskResponse> RunRegularTask(RunRegularTaskRequest request, HttpRequestBase httpRequestBase)
        {
            Func<RunRegularTaskResponse> x =
                () =>
                {
                    var result = new RunRegularTaskResponse();
                    DbFinanceWorker.RunRegularTask();
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.RunRegularTask);
        }

        public static Response<GetFastOperationsResponse> GetFastOperations(GetFastOperationsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetFastOperationsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetFastOperationsResponse();
                    var fastOperations = DbFinanceFastOperationWorker.GetFastOperations(userId);
                    result.FastOperations = new List<GetFastOperationsResponse.FastOperationValue>();
                    foreach (var fastOperation in fastOperations)
                    {
                        var fastOperationValue = new GetFastOperationsResponse.FastOperationValue();
                        fastOperationValue.Id = fastOperation.Id;
                        fastOperationValue.Id = fastOperation.Id;
                        fastOperationValue.PaymentType = fastOperation.PaymentType;
                        fastOperationValue.Name = fastOperation.Name;
                        fastOperationValue.Sum = fastOperation.Sum;
                        fastOperationValue.CategoryId = fastOperation.CategoryId;
                        fastOperationValue.Comment = fastOperation.Comment;
                        fastOperationValue.Place = fastOperation.Place;
                        fastOperationValue.Order = fastOperation.Order;

                        result.FastOperations.Add(fastOperationValue);
                    }

                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetFastOperations);
        }

        public static Response<GetFastOperationResponse> GetFastOperation(GetFastOperationRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetFastOperationResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetFastOperationResponse();
                    var fastOperation = DbFinanceFastOperationWorker.GetFastOperation(userId, request.FastOperationId);

                    var fastOperationValue = new GetFastOperationResponse.FastOperationValue();
                    fastOperationValue.Id = fastOperation.Id;
                    fastOperationValue.PaymentType = fastOperation.PaymentType;
                    fastOperationValue.Name = fastOperation.Name;
                    fastOperationValue.Sum = fastOperation.Sum;
                    fastOperationValue.CategoryId = fastOperation.CategoryId;
                    fastOperationValue.Comment = fastOperation.Comment;
                    fastOperationValue.Place = fastOperation.Place;
                    fastOperationValue.Order = fastOperation.Order;

                    result.FastOperation = fastOperationValue;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.GetFastOperation);
        }

        public static Response<CreateFastOperationResponse> CreateFastOperation(CreateFastOperationRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateFastOperationResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new CreateFastOperationResponse();

                    result.FastOperationId = DbFinanceFastOperationWorker.CreateFastOperation(userId, request.Name, request.Sum, request.CategoryId, request.Comment, request.Place, request.Order);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.CreateFastOperation);
        }

        public static Response<UpdateFastOperationResponse> UpdateFastOperation(UpdateFastOperationRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdateFastOperationResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdateFastOperationResponse();

                    DbFinanceFastOperationWorker.UpdateFastOperation(userId, request.Id, request.Name, request.Sum, request.CategoryId, request.Comment, request.Place, request.Order);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.UpdateFastOperation);
        }

        public static Response<DeleteFastOperationResponse> DeleteFastOperation(DeleteFastOperationRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteFastOperationResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new DeleteFastOperationResponse();
                    DbFinanceFastOperationWorker.DeleteFastOperation(userId, request.FastOperationId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.MoneyService.DeleteFastOperation);
        }

    }
}