using Common;
using Common.Enums;
using Common.Exceptions;
using Extentions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataWorker
{
    public class DbFinanceWorker
    {

        public static int CreateCategory(int userId, string name, string description, int? parentId, string color, PaymentTypes paymentType, int? order)
        {
            using (var context = new Data.DataContext())
            {
                if (parentId != null)
                {
                    var hasCategory = context.Categories.Any(x => x.UserId == userId && x.CategoryId == parentId && x.TypeId == ((int)paymentType));
                    if (!hasCategory)
                    {
                        throw new MessageException("parent category not found");
                    }
                }

                //todo need optimization in future
                var categoryId = context.Categories.Where(x => x.UserId == userId).Select(x => x.CategoryId).DefaultIfEmpty(0).Max() + 1;

                var dbCategory = new Data.Category();
                dbCategory.CategoryId = categoryId;
                dbCategory.UserId = userId;
                dbCategory.ParentId = parentId;
                dbCategory.Color = color;
                dbCategory.Description = description;
                dbCategory.Name = name;
                dbCategory.Order = order;
                dbCategory.TypeId = (int)paymentType;
                context.Categories.Add(dbCategory);
                context.SaveChanges();
                return categoryId;
            }
        }

        public static void UpdateCategory(int userId, int categoryId, string name, string description, int? parentId, string color, int? order)
        {
            using (var context = new Data.DataContext())
            {
                var dbCategory = context.Categories.SingleOrDefault(x => x.CategoryId == categoryId && x.UserId == userId);
                if (dbCategory == null)
                {
                    throw new MessageException("category not found");
                }

                if (parentId != null)
                {
                    var hasCategory = context.Categories.Any(x => x.UserId == userId && x.CategoryId == parentId && dbCategory.TypeId == x.TypeId);
                    if (!hasCategory)
                    {
                        throw new MessageException("parent category not found");
                    }
                }

                var nextParentId = parentId;
                while (true)
                {
                    if (nextParentId == null)
                    {
                        break;
                    }

                    var parent = context.Categories.Single(x => x.CategoryId == nextParentId && x.UserId == userId && dbCategory.TypeId == x.TypeId);
                    nextParentId = parent.ParentId;
                    if (nextParentId == categoryId)
                    {
                        throw new MessageException("recursive parents");
                    }
                }

                dbCategory.CategoryId = categoryId;
                dbCategory.ParentId = parentId;
                dbCategory.Color = color;
                dbCategory.Description = description;
                dbCategory.Name = name;
                dbCategory.Order = order;
                context.SaveChanges();
            }
        }


        public static void DeleteCategory(int userId, int categoryId)
        {
            using (var context = new Data.DataContext())
            {
                var dbCategory = context.Categories.SingleOrDefault(x => x.CategoryId == categoryId && x.UserId == userId);
                if (dbCategory == null)
                {
                    throw new MessageException("category not found");
                }

                var hasPayments = context.Payments.Any(x => x.CategoryId == categoryId && x.UserId == userId);
                if (hasPayments)
                {
                    throw new MessageException("has payments");
                }

                var hasChield = context.Categories.Any(x => x.ParentId == categoryId && x.UserId == userId);
                if (hasChield)
                {
                    throw new MessageException("is parent category");
                }

                context.Categories.Remove(dbCategory);
                context.SaveChanges();
            }
        }

        public static PaymentCategory GetCategory(int userId, int categoryId)
        {
            using (var context = new Data.DataContext())
            {
                var dbPayment = context.Categories.SingleOrDefault(x => x.CategoryId == categoryId && x.UserId == userId);
                if (dbPayment == null)
                {
                    throw new MessageException("category not found");
                }

                var category = new PaymentCategory();
                category.Id = dbPayment.CategoryId;
                category.ParentId = dbPayment.ParentId;
                category.Color = dbPayment.Color;
                category.Description = dbPayment.Description;
                category.Name = dbPayment.Name;
                category.Order = dbPayment.Order;
                category.PaymentType = (PaymentTypes)dbPayment.TypeId;
                return category;
            }
        }

        public static List<PaymentCategory> GetCategories(int userId, PaymentTypes? type)
        {
            using (var context = new Data.DataContext())
            {
                var dbCats = context.Categories.Where(x => x.UserId == userId);
                if (type != null)
                {
                    dbCats = dbCats.Where(x => x.TypeId == (int)type);
                }

                var dbCategories = dbCats.OrderBy(x => x.Order == null).ThenBy(x => x.Order).ThenBy(x => x.Name).ToList();
                var categories = new List<PaymentCategory>();
                foreach (var dbCategory in dbCategories)
                {
                    var category = new PaymentCategory();
                    category.Id = dbCategory.CategoryId;
                    category.Name = dbCategory.Name;
                    category.Description = dbCategory.Description;
                    category.Color = dbCategory.Color;
                    category.ParentId = dbCategory.ParentId;
                    category.Order = dbCategory.Order;
                    category.PaymentType = (PaymentTypes)dbCategory.TypeId;
                    categories.Add(category);
                }

                return categories;
            }
        }

        public static int CreatePayment(int userId, decimal sum, int categoryId, string comment, DateTime payDate, string place)
        {
            using (var context = new Data.DataContext())
            {
                var paymentId = CreatePayment(context, userId, sum, categoryId, comment, payDate, place);
                context.SaveChanges();
                return paymentId;
            }
        }

        public static int CreatePayment(Data.DataContext context, int userId, decimal sum, int categoryId, string comment, DateTime payDate, string place, int? regularTaskId = null)
        {
            var placeId = GetPlaceId(context, place, userId);
            return CreatePayment(context, userId, sum, categoryId, comment, payDate, placeId, regularTaskId);
        }

        public static int CreatePayment(Data.DataContext context, int userId, decimal sum, int categoryId, string comment, DateTime payDate, int? placeId, int? regularTaskId = null)
        {
            var category = GetCategory(context, categoryId, userId);
            comment = comment.TrimValue(' ');

            //todo need optimization in future
            var paymentId = context.Payments.Where(x => x.UserId == userId).Select(x => x.PaymentId).DefaultIfEmpty(0).Max() + 1;

            var dbPayment = new Data.Payment();
            dbPayment.CategoryId = categoryId;
            dbPayment.Sum = sum;
            dbPayment.TypeId = category.TypeId;
            dbPayment.Comment = comment;
            dbPayment.PaymentId = paymentId;
            dbPayment.UserId = userId;
            dbPayment.Date = payDate;
            dbPayment.PlaceId = placeId;
            dbPayment.CreatedTaskId = regularTaskId;
            context.Payments.Add(dbPayment);
            return paymentId;
        }

        public static Data.Category GetCategory(Data.DataContext context, int categoryId, int userId)
        {
            var category = context.Categories.FirstOrDefault(x => x.UserId == userId && x.CategoryId == categoryId);
            if (category == null)
            {
                throw new MessageException("категория не найдена");
            }

            return category;
        }

        public static int? GetPlaceId(Data.DataContext context, string place, int userId)
        {
            place = place.TrimValue(' ');
            if (string.IsNullOrEmpty(place))
            {
               return null;
            }

            var dbPlace = context.Places.FirstOrDefault(x => x.UserId == userId && x.Name == place);
            if (dbPlace == null)
            {
                //todo need optimization in future
                var newPlaceId = context.Places.Where(x => x.UserId == userId).Select(x => x.PlaceId).DefaultIfEmpty(0).Max() + 1;
                dbPlace = new Data.Place();
                dbPlace.UserId = userId;
                dbPlace.PlaceId = newPlaceId;
                context.Places.Add(dbPlace);
            }

            dbPlace.LastUsedDate = DateTime.Now;
            dbPlace.Name = place;
            return dbPlace.PlaceId;
        }

        public static void CheckRemovePlace(Data.DataContext context, int? placeId, int userId, int? paymentId, int? fastOperationId)
        {
            var dbPlace = placeId != null ? context.Places.First(x => x.UserId == userId && x.PlaceId == placeId) : null;
            if (dbPlace != null)
            {
                var hasAnyPayments = IsPlaceBusy(context, dbPlace.PlaceId, userId, paymentId, fastOperationId);
                if (!hasAnyPayments)
                {
                    context.Places.Remove(dbPlace);
                }
            }
        }

        public static bool IsPlaceBusy(Data.DataContext context, int placeId, int userId, int? paymentId, int? fastOperationId)
        {
            var hasAnyPayments = false;
            if (paymentId != null)
            {
                hasAnyPayments = context.Payments.Any(x => x.UserId == userId && x.PlaceId == placeId && x.PaymentId != paymentId);
            }
            else
            {
                hasAnyPayments = context.Payments.Any(x => x.UserId == userId && x.PlaceId == placeId);
            }
            if (!hasAnyPayments)
            {
                if (fastOperationId != null)
                {
                    hasAnyPayments = context.FastOperations.Any(x => x.UserId == userId && x.PlaceId == placeId && x.FastOperationId != fastOperationId);
                }
                else
                {
                    hasAnyPayments = context.FastOperations.Any(x => x.UserId == userId && x.PlaceId == placeId);
                }
            }
            return hasAnyPayments;
        }

        public static int? GetPlaceId(Data.DataContext context, string place, int userId, int? currentPlaceId, int? paymentId, int? fastOperationId)
        {
            var dbPlace = currentPlaceId != null ? context.Places.First(x => x.UserId == userId && x.PlaceId == currentPlaceId) : null;
            var hasAnyPayments = false;
            if (dbPlace != null)
            {
                hasAnyPayments = IsPlaceBusy(context, dbPlace.PlaceId, userId, paymentId, fastOperationId);
            }

            int? placeId = null;
            if (!string.IsNullOrEmpty(place))
            {
                var dbNewPlace = context.Places.FirstOrDefault(x => x.UserId == userId && x.Name == place);
                if (dbNewPlace == null)
                {
                    if (dbPlace != null && !hasAnyPayments)
                    {
                        dbNewPlace = dbPlace;
                        dbNewPlace.LastUsedDate = DateTime.Now;
                    }
                    else
                    {
                        //todo need optimization in future
                        var newPlaceId = context.Places.Where(x => x.UserId == userId).Select(x => x.PlaceId).DefaultIfEmpty(0).Max() + 1;
                        dbNewPlace = new Data.Place();
                        dbNewPlace.UserId = userId;
                        dbNewPlace.PlaceId = newPlaceId;
                        dbNewPlace.LastUsedDate = DateTime.Now;
                        context.Places.Add(dbNewPlace);
                    }
                }
                else
                {
                    dbNewPlace.LastUsedDate = DateTime.Now;
                    if (dbPlace != null && !hasAnyPayments && dbPlace.PlaceId != dbNewPlace.PlaceId)
                    {
                        context.Places.Remove(dbPlace);
                    }
                }

                dbNewPlace.Name = place;
                placeId = dbNewPlace.PlaceId;
            }
            else
            {
                placeId = null;
                if (dbPlace != null && !hasAnyPayments)
                {
                    context.Places.Remove(dbPlace);
                }
            }

            return placeId;
        }

        public static void UpdatePayment(int userId, int paymentId, decimal sum, int categoryId, string comment, DateTime payDate, string place)
        {
            using (var context = new Data.DataContext())
            {
                var dbPayment = context.Payments.SingleOrDefault(x => x.PaymentId == paymentId && x.UserId == userId && x.TaskId == null);
                if (dbPayment == null)
                {
                    throw new MessageException("payment not found");
                }
                var category = GetCategory(context, categoryId, userId);

                comment = comment.TrimValue(' ');
                place = place.TrimValue(' ');
                var placeId = GetPlaceId(context, place, userId, dbPayment.PlaceId, paymentId, null);

                dbPayment.TypeId = category.TypeId;
                dbPayment.CategoryId = categoryId;
                dbPayment.Sum = sum;
                dbPayment.Comment = comment;
                dbPayment.Date = payDate;
                dbPayment.PlaceId = placeId;
                context.SaveChanges();
            }
        }

        public static void UpdatePaymentsBatch(int userId, List<int> paymentIds, int categoryId)
        {
            using (var context = new Data.DataContext())
            {
                var category = context.Categories.FirstOrDefault(x => x.UserId == userId && x.CategoryId == categoryId);
                if (category == null)
                {
                    throw new MessageException("category not found");
                }

                var dbPayments = context.Payments.Where(x => paymentIds.Contains(x.PaymentId) && x.UserId == userId && x.TaskId == null).ToList();

                if(dbPayments.Count != paymentIds.Count)
                {
                    throw new MessageException("one ore more payments not found");
                }

                foreach (var dbPayment in dbPayments)
                {
                    dbPayment.TypeId = category.TypeId;
                    dbPayment.CategoryId = categoryId;
                }

                context.SaveChanges();
            }
        }
        

        public static void DeletePayment(int userId, int paymentId)
        {
            using (var context = new Data.DataContext())
            {
                var dbPayment = context.Payments.SingleOrDefault(x => x.PaymentId == paymentId && x.UserId == userId && x.TaskId == null);
                if (dbPayment == null)
                {
                    throw new MessageException("payment not found");
                }

                context.Payments.Remove(dbPayment);
                CheckRemovePlace(context, dbPayment.PlaceId, userId, dbPayment.PaymentId, null);
                context.SaveChanges();
            }
        }

        public static Payment GetPayment(int userId, int paymentId)
        {
            using (var context = new Data.DataContext())
            {
                var dbPayment = context.Payments.SingleOrDefault(x => x.PaymentId == paymentId && x.UserId == userId && x.TaskId == null);
                if (dbPayment == null)
                {
                    throw new MessageException("payment not found");
                }
                var payment = new Payment();
                payment.CategoryId = dbPayment.CategoryId;
                payment.Sum = dbPayment.Sum;
                payment.PaymentType = (PaymentTypes)dbPayment.TypeId;
                payment.Comment = dbPayment.Comment;
                payment.Place = dbPayment.PlaceId != null ? context.Places.First(x => x.UserId == userId && x.PlaceId == dbPayment.PlaceId).Name : null;
                payment.CreatedTaskId = dbPayment.CreatedTaskId;
                payment.Id = dbPayment.PaymentId;
                payment.Date = dbPayment.Date;
                return payment;
            }
        }

        public static List<Payment> GetPayments(int userId, DateTime? dateFrom, DateTime? dateTo, List<int> categoryIds, string comment, string place, int? dateFromPaymentId)
        {
            using (var context = new Data.DataContext())
            {
                var dbPayments = context.Payments.Where(x => x.UserId == userId && x.TaskId == null).AsQueryable();
                if (dateFromPaymentId != null)
                {
                    var dbPaym = context.Payments.SingleOrDefault(x => x.UserId == userId && x.PaymentId == dateFromPaymentId);
                    if (dbPaym != null)
                    {
                        dateFrom = dbPaym.Date.Date;
                        dateTo = dateFrom.Value.AddDays(1);
                    }
                }
                if (dateFrom != null)
                {
                    dbPayments = dbPayments.Where(x => x.Date >= dateFrom);
                }
                if (dateTo != null)
                {
                    dbPayments = dbPayments.Where(x => x.Date < dateTo);
                }
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    dbPayments = dbPayments.Where(x => x.CategoryId != null && categoryIds.Contains(x.CategoryId.Value));
                }
                if (!String.IsNullOrEmpty(comment))
                {
                    dbPayments = dbPayments.Where(x => x.Comment != null && x.Comment.Contains(comment));
                }
                if (!String.IsNullOrEmpty(place))
                {
                    var placesIds = context.Places.Where(x => x.UserId == userId && x.Name.Contains(place)).Select(x => x.PlaceId);
                    dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
                }
                var placeIds = dbPayments.Where(x=>x.PlaceId != null).Select(x => x.PlaceId.Value);
                var dbPlaces = context.Places.Where(x => x.UserId == userId && placeIds.Contains(x.PlaceId)).ToList();
                var dbPaymentList = dbPayments.OrderByDescending(x => x.Date).ThenBy(x => x.CategoryId).ToList();
                var payments = new List<Payment>();
                foreach (var dbPayment in dbPaymentList)
                {
                    var payment = new Payment();
                    payment.CategoryId = dbPayment.CategoryId;
                    payment.Sum = dbPayment.Sum;
                    payment.PaymentType = (PaymentTypes)dbPayment.TypeId;
                    payment.Comment = dbPayment.Comment;
                    payment.Place = dbPayment.PlaceId != null ? dbPlaces.First(x => x.PlaceId == dbPayment.PlaceId).Name : null;
                    payment.Id = dbPayment.PaymentId;
                    payment.Date = dbPayment.Date;
                    payment.CreatedTaskId = dbPayment.CreatedTaskId;
                    payments.Add(payment);
                }
                return payments;
            }
        }

        public static List<Place> GetPlaces(int userId, string name, int offset, int count, string sortby)
        {
            using (var context = new Data.DataContext())
            {
                // ned optimization!
                var dbPlaces = context.Places.Where(x => x.UserId == userId && x.Name.Contains(name)).ToList();
                if (sortby.ToLower() == "startwith,lastuseddate desc")
                {
                    dbPlaces = dbPlaces.OrderBy(x=>x.Name.StartsWith(name)).ThenByDescending(x => x.LastUsedDate).ToList();
                }
                else
                {
                    dbPlaces = dbPlaces.OrderBy(x => x.Name).ToList();
                }
                var dbPlacesList = dbPlaces.Skip(offset).Take(count).ToList();
                var places = dbPlaces.Select(x => new Place { Id = x.PlaceId, Name = x.Name }).ToList();
                return places;                
            }
        }

        public static List<Debt> GetDebts(int userId, bool withPaid = false)
        {
            using (var context = new Data.DataContext())
            {
                var dbDetsQueryble = context.Debs.Where(x => x.UserId == userId);
                if (withPaid)
                {
                    dbDetsQueryble = dbDetsQueryble.Where(x => x.StatusId == (int)DebtStatus.Actual || x.StatusId == (int)DebtStatus.Paid);
                }
                else
                {
                    dbDetsQueryble = dbDetsQueryble.Where(x => x.StatusId == (int)DebtStatus.Actual);
                }

                var dbDebts = dbDetsQueryble.ToList();
                var dbDebtUsers = context.DebtUsers.Where(x => x.UserId == userId).ToList();
                var debts = new List<Debt>();
                foreach (var dbDebt in dbDebts)
                {
                    var debt = new Debt();
                    var dbDebtUser = dbDebtUsers.Single(x => x.UserId == userId && x.DebtUserId == dbDebt.DebtUserId);
                    SetDebtFields(debt, dbDebt, dbDebtUser);
                    debts.Add(debt);
                }
                return debts;
            }
        }

        public static Debt GetDebt(int userId, int debtId)
        {
            using (var context = new Data.DataContext())
            {
                var dbDebt = context.Debs.SingleOrDefault(x => x.UserId == userId && x.DebtId == debtId && x.StatusId != (int)DebtStatus.Deleted);
                if (dbDebt == null)
                {
                    throw new MessageException("Долг не найден");
                }
                var dbDebtUser = context.DebtUsers.Single(x => x.UserId == userId && x.DebtUserId == dbDebt.DebtUserId);
                var debt = new Debt();
                SetDebtFields(debt, dbDebt, dbDebtUser);
                return debt;
            }
        }

        private static void SetDebtFields(Debt debt, Data.Debt dbDebt, Data.DebtUser dbDebtUser)
        {
            debt.Type = (DebtTypes)dbDebt.Type;
            debt.Sum = dbDebt.Sum;
            debt.DebtUser = new DebtUser();
            debt.DebtUser.Id = dbDebtUser.Id;
            debt.DebtUser.Name = dbDebtUser.Name;
            debt.Comment = dbDebt.Comment;
            debt.Id = dbDebt.DebtId;
            debt.Date = dbDebt.Date;
            debt.PaySum = dbDebt.PaySum;
            debt.PayComment = dbDebt.PayComment;
            debt.Status = (DebtStatus)dbDebt.StatusId;
        }

        public static int CreateDebt(int userId, decimal sum, DebtTypes type, DateTime date, string name, string comment)
        {
            using (var context = new Data.DataContext())
            {
                if (sum < 0)
                {
                    throw new MessageException("отрицательная сумма недопустима");
                }
                var debtUser = context.DebtUsers.FirstOrDefault(x => x.UserId == userId && x.Name == name);
                if (debtUser == null)
                {
                    //todo need optimization in future
                    var debtUserId = context.DebtUsers.Where(x => x.UserId == userId).Select(x => x.DebtUserId).DefaultIfEmpty(0).Max() + 1;
                    debtUser = new Data.DebtUser();
                    debtUser.UserId = userId;
                    debtUser.Name = name;
                    debtUser.DebtUserId = debtUserId;
                    context.DebtUsers.Add(debtUser);
                }

                //todo need optimization in future
                var debtId = context.Debs.Where(x => x.UserId == userId).Select(x => x.DebtId).DefaultIfEmpty(0).Max() + 1;
                var dbDebt = new Data.Debt();
                dbDebt.Sum = sum;
                dbDebt.Type = (int)type;
                dbDebt.DebtUserId = debtUser.DebtUserId;
                dbDebt.Comment = comment;
                dbDebt.UserId = userId;
                dbDebt.DebtId = debtId;
                dbDebt.Date = date;
                dbDebt.PaySum = 0;
                dbDebt.StatusId = (int)DebtStatus.Actual;
                context.Debs.Add(dbDebt);
                context.SaveChanges();
                return debtId;
            }
        }

        public static void PayDebt(int userId, int debtId, decimal sum, DateTime date, string comment)
        {
            using (var context = new Data.DataContext())
            {
                var dbDebt = context.Debs.SingleOrDefault(x => x.UserId == userId && x.DebtId == debtId && x.StatusId != (int)DebtStatus.Deleted);
                if (dbDebt == null)
                {
                    throw new MessageException("debt not found");
                }
                if (dbDebt.Sum < dbDebt.PaySum + sum)
                {
                    throw new MessageException("PaySum > need pay");
                }
                dbDebt.PaySum += sum;
                if (dbDebt.PaySum == dbDebt.Sum)
                {
                    dbDebt.StatusId = (int)DebtStatus.Paid;
                }
                if (!String.IsNullOrEmpty(dbDebt.PayComment))
                {
                    dbDebt.PayComment += Environment.NewLine;
                }
                dbDebt.PayComment += date.ToString("yyyy.MM.dd") + " " + sum + " " + comment;
                context.SaveChanges();
            }
        }

        public static void MergeDebtUsers(int userId, int fromUserId, int toUserId)
        {
            using (var context = new Data.DataContext())
            {
                var dbFromUser = context.DebtUsers.FirstOrDefault(x => x.UserId == userId && x.DebtUserId == fromUserId);
                if (dbFromUser == null)
                {
                    throw new MessageException("Сливаемый не найден");
                }
                var dbToUser = context.DebtUsers.FirstOrDefault(x => x.UserId == userId && x.DebtUserId == toUserId);
                if (dbToUser == null)
                {
                    throw new MessageException("Поглощающий не найден");
                }
                if (fromUserId == toUserId)
                {
                    throw new MessageException("Нужно выбрать разных держателей долга");
                }
                var dbDebts = context.Debs.Where(x => x.DebtUserId == dbFromUser.DebtUserId && x.UserId == userId);
                foreach (var dbDebt in dbDebts)
                {
                    dbDebt.DebtUserId = toUserId;
                }
                context.DebtUsers.Remove(dbFromUser);
                context.SaveChanges();
            }
        }

        public static List<DebtUser> GetDebtUsers(int userId, string search, out int totalCount)
        {
            using (var context = new Data.DataContext())
            {
                var dbDebtUserIdsByDebts = context.Debs.Where(x => x.UserId == userId && x.StatusId != (int)DebtStatus.Deleted).Select(x => x.DebtUserId);
                var dbDebtUsers = context.DebtUsers.Where(x => x.UserId == userId && dbDebtUserIdsByDebts.Contains(x.DebtUserId));
                totalCount = dbDebtUsers.Count();
                if (!string.IsNullOrEmpty(search))
                {
                    dbDebtUsers = dbDebtUsers.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }
                var debtUsers = new List<DebtUser>();
                foreach (var dbDebtUser in dbDebtUsers.ToList())
                {
                    var debtUser = new DebtUser();
                    debtUser.Id = dbDebtUser.Id;
                    debtUser.Name = dbDebtUser.Name;
                    debtUsers.Add(debtUser);
                }

                return debtUsers;
            }
        }

        public static void MoveDebtToOperations(int userId, List<int> debtIds, int categoryId, string comment)
        {
            using (var context = new Data.DataContext())
            {
                var dbDebts = context.Debs.Where(x => x.UserId == userId && debtIds.Contains(x.DebtId) && x.StatusId == (int)DebtStatus.Actual).ToList();
                if (dbDebts.Count == 0)
                {
                    throw new MessageException("Ни один долг не найден или статус долга не соответствует");
                }
                if (dbDebts.Any(x => x.Type == (int)DebtTypes.Minus))
                {
                    throw new MessageException("Перемещать можно только долги, которые долны вам");
                }
                var hasCategory = context.Categories.Any(x => x.UserId == userId && x.CategoryId == categoryId);
                if (!hasCategory)
                {
                    throw new MessageException("категория не найдена");
                }
                var dbDebtUsers = context.DebtUsers.Where(x => x.UserId == userId).ToList();
                foreach (var dbDebt in dbDebts)
                {
                    var paymentId = context.Payments.Where(x => x.UserId == userId).Select(x => x.PaymentId).DefaultIfEmpty(0).Max() + 1;
                    var dbPayment = new Data.Payment();
                    dbPayment.CategoryId = categoryId;
                    dbPayment.Sum = dbDebt.Sum - dbDebt.PaySum;
                    dbPayment.TypeId = (int)PaymentTypes.Costs;
                    var dbDebtUser = dbDebtUsers.Single(x => x.UserId == userId && x.DebtUserId == dbDebt.DebtUserId);

                    dbPayment.Comment = comment + dbDebtUser.Name + " сумма долга: " + dbDebt.Sum + " из них оплачено: " + dbDebt.PaySum;
                    if (!String.IsNullOrEmpty(dbDebt.Comment))
                    {
                        dbPayment.Comment += Environment.NewLine + "комментарий:" + dbDebt.Comment;
                    }
                    if (!String.IsNullOrEmpty(dbDebt.PayComment))
                    {
                        dbPayment.Comment += Environment.NewLine + "платёжный комментарий:" + dbDebt.PayComment;
                    }
                    dbPayment.PaymentId = paymentId;
                    dbPayment.UserId = userId;
                    dbPayment.Date = dbDebt.Date;
                    context.Payments.Add(dbPayment);
                    context.Debs.Remove(dbDebt);
                    context.SaveChanges();
                }
            }
        }

        public static void DeleteDebt(int userId, int debtId)
        {
            using (var context = new Data.DataContext())
            {
                var dbDebt = context.Debs.SingleOrDefault(x => x.UserId == userId && x.DebtId == debtId && x.StatusId != (int)DebtStatus.Deleted);
                if (dbDebt == null)
                {
                    throw new MessageException("Долг не найден");
                }
                dbDebt.StatusId = (int)DebtStatus.Deleted;
                context.SaveChanges();
            }
        }

        public static void UpdateDebt(int userId, int debtId, decimal sum, string name, string comment, DateTime date)
        {
            using (var context = new Data.DataContext())
            {
                var dbDebt = context.Debs.SingleOrDefault(x => x.UserId == userId && x.DebtId == debtId && x.StatusId != (int)DebtStatus.Deleted);
                if (dbDebt == null)
                {
                    throw new MessageException("Долг не найден");
                }
                if (dbDebt.StatusId != (int)DebtStatus.Actual)
                {
                    throw new MessageException("update only actual debts");
                }
                if (sum < 0)
                {
                    throw new MessageException("sum < 0");
                }
                if (dbDebt.PaySum > 0 && dbDebt.PaySum >= sum)
                {
                    throw new MessageException("paysum >= debtSum");
                }
                var debtUser = context.DebtUsers.FirstOrDefault(x => x.UserId == userId && x.Name == name);
                if (debtUser == null)
                {
                    //todo need optimization in future
                    var debtUserId = context.DebtUsers.Where(x => x.UserId == userId).Select(x => x.DebtUserId).DefaultIfEmpty(0).Max() + 1;
                    debtUser = new Data.DebtUser();
                    debtUser.UserId = userId;
                    debtUser.Name = name;
                    debtUser.DebtUserId = debtUserId;
                    context.DebtUsers.Add(debtUser);
                }

                dbDebt.Comment = comment;
                dbDebt.Sum = sum;
                dbDebt.DebtUserId = debtUser.DebtUserId;
                dbDebt.Date = date;
                context.SaveChanges();
            }
        }

        public static PaymentStatistics GetPaymentStatisticsResponse(int userId, DateTime? dateFrom, DateTime? dateTo)
        {
            using (var context = new Data.DataContext())
            {
                var dbPayments = context.Payments.Where(x => x.UserId == userId);
                if (dateFrom != null)
                {
                    dbPayments = dbPayments.Where(x => x.Date >= dateFrom);
                }
                if (dateTo != null)
                {
                    dbPayments = dbPayments.Where(x => x.Date < dateTo);
                }
                return new PaymentStatistics();
            }
        }

        public static List<RegularTask> GetRegularTasks(int userId)
        {
            using (var context = new Data.DataContext())
            {
                var dbRegularTasks = context.RegularTasks.Where(x => x.UserId == userId).ToList();
                var taskIds = dbRegularTasks.Select(x => x.TaskId).ToList();
                var dbPayments = context.Payments.Where(x => x.UserId == userId && x.TaskId != null && taskIds.Contains(x.TaskId.Value)).ToList();
                var placeIds = dbPayments.Where(x => x.PlaceId != null).Select(x => x.PlaceId.Value);
                var dbPlaces = context.Places.Where(x => x.UserId == userId && placeIds.Contains(x.PlaceId)).ToList();
                var reqularTasks = new List<RegularTask>();
                foreach (var dbRegularTask in dbRegularTasks)
                {
                    var regularTask = new RegularTask();
                    var dbPayment = dbPayments.SingleOrDefault(x => x.TaskId == dbRegularTask.TaskId);
                    var place = dbPayment.PlaceId != null ? dbPlaces.First(x => x.PlaceId == dbPayment.PlaceId).Name : null;
                    SetRegularTaskFields(regularTask, dbRegularTask, dbPayment, place);
                    reqularTasks.Add(regularTask);
                }
                return reqularTasks;
            }
        }

        public static RegularTask GetRegularTask(int userId, int regularTaskId)
        {
            using (var context = new Data.DataContext())
            {
                var dbRegularTask = context.RegularTasks.SingleOrDefault(x => x.UserId == userId && x.TaskId == regularTaskId);
                if (dbRegularTask == null)
                {
                    throw new MessageException("regular task not found");
                }

                var dbPayment = context.Payments.SingleOrDefault(x => x.UserId == userId && x.TaskId == dbRegularTask.TaskId);
                var regularTask = new RegularTask();
                var place = dbPayment.PlaceId != null ? context.Places.First(x => x.UserId == userId && x.PlaceId == dbPayment.PlaceId.Value).Name : null;
                SetRegularTaskFields(regularTask, dbRegularTask, dbPayment, place);
                return regularTask;
            }
        }

        private static void SetRegularTaskFields(RegularTask regularTask, Data.RegularTask dbRegularTask, Data.Payment dbPayment, string place)
        {
            regularTask.Id = dbRegularTask.TaskId;
            regularTask.Type = (RegularTaskTypes)dbRegularTask.TypeId;
            regularTask.Name = dbRegularTask.Name;
            regularTask.TimeType = (RegularTaskTimeTypes)dbRegularTask.TimeId;
            regularTask.TimeValue = dbRegularTask.TimeValue;
            regularTask.DateFrom = dbRegularTask.DateFrom;
            regularTask.DateTo = dbRegularTask.DateTo;
            regularTask.RunTime = dbRegularTask.RunTime;

            if (dbPayment != null)
            {
                regularTask.Payment = new Payment();
                regularTask.Payment.CategoryId = dbPayment.CategoryId;
                regularTask.Payment.Sum = dbPayment.Sum;
                regularTask.Payment.PaymentType = (PaymentTypes)dbPayment.TypeId;
                regularTask.Payment.Comment = dbPayment.Comment;
                regularTask.Payment.Place = place;
            }
        }

        public static int CreateReqularTask(int userId, string name, RegularTaskTypes type, RegularTaskTimeTypes timeType, int? timeValue, DateTime dateFrom, DateTime? dateTo,
            Payment payment)
        {
            using (var context = new Data.DataContext())
            {
                if (context.RegularTasks.Any(x => x.UserId == userId && x.Name == name))
                {
                    throw new MessageException("наименование занято");
                }
                //todo need optimization in future
                var taskId = context.RegularTasks.Where(x => x.UserId == userId).Select(x => x.TaskId).DefaultIfEmpty(0).Max() + 1;

                var dbTask = new Data.RegularTask();
                dbTask.UserId = userId;
                dbTask.TaskId = taskId;
                dbTask.TypeId = (int)type;
                dbTask.Name = name;
                dbTask.TimeId = (int)timeType;
                dbTask.TimeValue = timeValue;
                dbTask.DateFrom = dateFrom;
                dbTask.DateTo = dateTo;
                CheckTime(timeType, timeValue);
                dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, DateTime.Now.Date, timeType, timeValue);
                context.RegularTasks.Add(dbTask);

                if (type == RegularTaskTypes.Operation)
                {
                    //todo need optimization in future
                    var paymentId = context.Payments.Where(x => x.UserId == userId).Select(x => x.PaymentId).DefaultIfEmpty(0).Max() + 1;

                    var dbCategory = GetCategory(context, payment.CategoryId.Value, userId);
                    var placeId = GetPlaceId(context, payment.Place, userId);

                    var dbPayment = new Data.Payment();
                    dbPayment.UserId = userId;
                    dbPayment.PaymentId = paymentId;
                    dbPayment.TaskId = taskId;
                    dbPayment.Sum = payment.Sum;
                    dbPayment.CategoryId = dbCategory.CategoryId;
                    dbPayment.TypeId = dbCategory.TypeId;
                    dbPayment.PlaceId = placeId;
                    dbPayment.Comment = payment.Comment;
                    dbPayment.Date = DateTime.Now;
                    context.Payments.Add(dbPayment);
                }

                context.SaveChanges();
                return taskId;
            }
        }

        public static void UpdateReqularTask(int userId, int id, string name, RegularTaskTimeTypes timeType, int? timeValue, DateTime dateFrom, DateTime? dateTo, Payment payment)
        {
            using (var context = new Data.DataContext())
            {
                var dbTask = context.RegularTasks.SingleOrDefault(x => x.UserId == userId && x.TaskId == id);
                if (dbTask == null)
                {
                    throw new MessageException("regular task not found");
                }

                dbTask.Name = name;
                dbTask.TimeId = (int)timeType;
                dbTask.TimeValue = timeValue;
                dbTask.DateFrom = dateFrom;
                dbTask.DateTo = dateTo;
                CheckTime(timeType, timeValue);
                dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, DateTime.Now.Date, timeType, timeValue);

                if (dbTask.TypeId == (int)RegularTaskTypes.Operation)
                {
                    var dbPayment = context.Payments.Single(x => x.UserId == dbTask.UserId && x.TaskId == dbTask.TaskId);
                    var dbCategory = GetCategory(context, payment.CategoryId.Value, userId);
                    var placeId = GetPlaceId(context, payment.Place, userId, dbPayment.PlaceId, dbPayment.PaymentId, null);

                    dbPayment.Sum = payment.Sum;
                    dbPayment.CategoryId = dbCategory.CategoryId;
                    dbPayment.TypeId = dbCategory.TypeId;
                    dbPayment.PlaceId = placeId;
                    dbPayment.Comment = payment.Comment;
                    dbPayment.Date = DateTime.Now;
                }
                context.SaveChanges();
            }
        }

        private static void CheckTime(RegularTaskTimeTypes timeType, int? timeValue)
        {
            if (timeType == RegularTaskTimeTypes.EveryDay)
            {
                if (timeValue != null)
                {
                    throw new MessageException("недопустимое значение интервала");
                }
            }

            if (timeType == RegularTaskTimeTypes.EveryWeek)
            {
                if (timeValue == null || timeValue < 1 || timeValue > 7)
                {
                    throw new MessageException("недопустимое значение интервала");
                }
            }

            if (timeType == RegularTaskTimeTypes.EveryMonth)
            {
                if (timeValue == null || timeValue < 1 || timeValue > 31)
                {
                    throw new MessageException("недопустимое значение интервала");
                }
            }
        }

        private static DateTime? GetRegularTaskRunTime(DateTime dateFrom, DateTime? dateTo, DateTime dateNow, RegularTaskTimeTypes timeType, int? timeValue)
        {
            var dn = dateNow;
            var date = dateFrom;
            if (dn > date)
            {
                date = dn;
            }

            if (dateTo < dn)
            {
                return null;
            }

            if (timeType == RegularTaskTimeTypes.EveryDay)
            {
                date = date.AddDays(1);
                return date.Date;
            }

            if (timeType == RegularTaskTimeTypes.EveryWeek)
            {
                date = date.AddDays(1);
                if (timeValue == 7)
                {
                    timeValue = 0;
                }
                return date.Date.GetNextWeekday((DayOfWeek)timeValue);
            }

            if (timeType == RegularTaskTimeTypes.EveryMonth)
            {
                var dt = date.Date;
                if (dt.Day < timeValue)
                {
                    dt = new DateTime(date.Year, date.Month, 1);
                }
                else
                {
                    dt = new DateTime(date.Year, date.Month, 1).AddMonths(1);
                }

                var nextDt = dt.AddDays(timeValue.Value - 1);
                if (dt.Month < nextDt.Month)
                {
                    dt = dt.AddMonths(1).AddDays(-1);
                }
                else
                {
                    dt = nextDt;
                }

                return dt;
            }

            throw new Exception("тип не определён");
        }

        public static void DeleteReqularTask(int userId, int regularTaskId)
        {
            using (var context = new Data.DataContext())
            {
                var dbTask = context.RegularTasks.SingleOrDefault(x => x.UserId == userId && x.TaskId == regularTaskId);
                if (dbTask == null)
                {
                    throw new MessageException("regular task not found");
                }

                context.RegularTasks.Remove(dbTask);

                if (dbTask.TypeId == (int)RegularTaskTypes.Operation)
                {
                    var dbPayment = context.Payments.Single(x => x.UserId == dbTask.UserId && x.TaskId == dbTask.TaskId);
                    context.Payments.Remove(dbPayment);
                    CheckRemovePlace(context, dbPayment.PlaceId, userId, dbPayment.PaymentId, null);
                }

                context.SaveChanges();
            }
        }

        public static void RunRegularTask()
        {
            using (var context = new Data.DataContext())
            {
                var dn = DateTime.Now.Date;
                var dbTasks = context.RegularTasks.Where(x => x.RunTime != null && x.RunTime <= dn).ToList();
                foreach (var dbTask in dbTasks)
                {
                    if (dbTask.TypeId == (int)RegularTaskTypes.Operation)
                    {
                        var dbt = context.Payments.Single(x => x.UserId == dbTask.UserId && x.TaskId == dbTask.TaskId);
                        CreatePayment(context, dbTask.UserId, dbt.Sum, dbt.CategoryId.Value, dbt.Comment, dbTask.RunTime.Value, dbt.PlaceId, dbTask.TaskId);
                    }

                    dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, dbTask.RunTime.Value, (RegularTaskTimeTypes)dbTask.TimeId, dbTask.TimeValue);
                    context.SaveChanges();
                }
            }
        }
    }
}