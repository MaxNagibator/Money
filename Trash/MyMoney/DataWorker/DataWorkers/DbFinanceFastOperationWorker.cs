using Common;
using Common.Enums;
using Common.Exceptions;
using Extentions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataWorker
{
    public class DbFinanceFastOperationWorker
    {
        public static List<FastOperation> GetFastOperations(int userId)
        {
            using (var context = new Data.DataContext())
            {
                var dbFastOperations = context.FastOperations.Where(x => x.UserId == userId).ToList();
                var taskIds = dbFastOperations.Select(x => x.FastOperationId).ToList();
                var placeIds = dbFastOperations.Where(x => x.PlaceId != null).Select(x => x.PlaceId.Value);
                var dbPlaces = context.Places.Where(x => x.UserId == userId && placeIds.Contains(x.PlaceId)).ToList();
                var reqularTasks = new List<FastOperation>();
                foreach (var dbFastOperation in dbFastOperations)
                {
                    var fastOperation = new FastOperation();
                    var place = dbFastOperation.PlaceId != null ? dbPlaces.First(x => x.PlaceId == dbFastOperation.PlaceId).Name : null;
                    SetFastOperationFields(fastOperation, dbFastOperation,place);
                    reqularTasks.Add(fastOperation);
                }
                return reqularTasks;
            }
        }

        public static FastOperation GetFastOperation(int userId, int fastOperationId)
        {
            using (var context = new Data.DataContext())
            {
                var dbFastOperation = context.FastOperations.SingleOrDefault(x => x.UserId == userId && x.FastOperationId == fastOperationId);
                if (dbFastOperation == null)
                {
                    throw new MessageException("fast operation not found");
                }

                var fastOperation = new FastOperation();
                var place = dbFastOperation.PlaceId != null ? context.Places.First(x => x.UserId == userId && x.PlaceId == dbFastOperation.PlaceId.Value).Name : null;
                SetFastOperationFields(fastOperation, dbFastOperation, place);
                return fastOperation;
            }
        }

        private static void SetFastOperationFields(FastOperation fastOperation, Data.FastOperation dbFastOperation,  string place)
        {
            fastOperation.Id = dbFastOperation.FastOperationId;
            fastOperation.Name = dbFastOperation.Name;
            fastOperation.CategoryId = dbFastOperation.CategoryId;
            fastOperation.Sum = dbFastOperation.Sum;
            fastOperation.PaymentType = (PaymentTypes)dbFastOperation.TypeId;
            fastOperation.Comment = dbFastOperation.Comment;
            fastOperation.Order = dbFastOperation.Order;
            fastOperation.Place = place;
        }

        public static int CreateFastOperation(int userId, string name,
            decimal sum, int categoryId, string comment, string place, int? order)
        {
            using (var context = new Data.DataContext())
            {
                if (context.FastOperations.Any(x => x.UserId == userId && x.Name == name))
                {
                    throw new MessageException("наименование занято");
                }

                comment = comment.TrimValue(' ');
                var category = DbFinanceWorker.GetCategory(context, categoryId, userId);

                //todo need optimization in future
                var fastOperationId = context.FastOperations.Where(x => x.UserId == userId).Select(x => x.FastOperationId).DefaultIfEmpty(0).Max() + 1;

                var dbFastOperation = new Data.FastOperation();
                dbFastOperation.UserId = userId;
                dbFastOperation.FastOperationId = fastOperationId;
                dbFastOperation.TypeId = category.TypeId;
                dbFastOperation.Name = name;
                dbFastOperation.Sum = sum;
                dbFastOperation.CategoryId = categoryId;
                dbFastOperation.Comment = comment;
                dbFastOperation.Order = order;
                dbFastOperation.PlaceId = DbFinanceWorker.GetPlaceId(context, place, userId);
                context.FastOperations.Add(dbFastOperation);

                context.SaveChanges();
                return fastOperationId;
            }
        }

        public static void UpdateFastOperation(int userId, int id, string name,
            decimal sum, int categoryId, string comment, string place, int? order)
        {
            using (var context = new Data.DataContext())
            {
                var dbFastOperation = context.FastOperations.SingleOrDefault(x => x.UserId == userId && x.FastOperationId == id);
                if (dbFastOperation == null)
                {
                    throw new MessageException("fast operation not found");
                }

                comment = comment.TrimValue(' ');
                var category = DbFinanceWorker.GetCategory(context, categoryId, userId);
                var placeId = DbFinanceWorker.GetPlaceId(context, place, userId, dbFastOperation.PlaceId, null, dbFastOperation.FastOperationId);

                dbFastOperation.TypeId = category.TypeId;
                dbFastOperation.Name = name;
                dbFastOperation.Sum = sum;
                dbFastOperation.CategoryId = categoryId;
                dbFastOperation.Comment = comment;
                dbFastOperation.Order = order;

                dbFastOperation.PlaceId = DbFinanceWorker.GetPlaceId(context, place, userId);

                context.SaveChanges();
            }
        }

        public static void DeleteFastOperation(int userId, int fastOperationId)
        {
            using (var context = new Data.DataContext())
            {
                var dbFastOperation = context.FastOperations.SingleOrDefault(x => x.UserId == userId && x.FastOperationId == fastOperationId);
                if (dbFastOperation == null)
                {
                    throw new MessageException("fast operation not found");
                }

                context.FastOperations.Remove(dbFastOperation);
                DbFinanceWorker.CheckRemovePlace(context, dbFastOperation.PlaceId, userId, null, dbFastOperation.FastOperationId);
                context.SaveChanges();
            }
        }
    }
}
