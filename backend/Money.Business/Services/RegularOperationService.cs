using Money.Data.Extensions;

namespace Money.Business.Services;

public class RegularOperationService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService,
    CategoryService categoryService,
    PlaceService placeService,
    OperationService operationService)
{
    public async Task<IEnumerable<RegularOperation>> GetAsync(CancellationToken cancellationToken)
    {
        IQueryable<Data.Entities.RegularOperation> dbOperations = context.RegularOperations
            .IsUserEntity(environment.UserId);

        List<int> placeIds = await dbOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<Place> places = await placeService.GetPlacesAsync(placeIds, cancellationToken);

        List<Data.Entities.RegularOperation> operations = await dbOperations
            .OrderBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return operations.Select(x => GetBusinessModel(x, places)).ToList();
    }

    public async Task<RegularOperation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.RegularOperation dbOperation = await GetByIdInternal(id, cancellationToken);

        List<Place>? places = null;

        if (dbOperation.PlaceId != null)
        {
            places = await placeService.GetPlacesAsync([dbOperation.PlaceId.Value], cancellationToken);
        }

        return GetBusinessModel(dbOperation, places);
    }

    public async Task<int> CreateAsync(RegularOperation operation, CancellationToken cancellationToken)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }
        CheckTime(operation.TimeType, operation.TimeValue);

        Data.Entities.DomainUser dbUser = await userService.GetCurrent(cancellationToken);
        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);

        int operationId = dbUser.NextRegularOperationId;
        dbUser.NextRegularOperationId++;

        int? placeId = await placeService.GetPlaceIdAsync(operation.Place, cancellationToken);

        Data.Entities.RegularOperation dbOperation = new()
        {
            Id = operationId,
            Name = operation.Name,
            UserId = environment.UserId.Value,
            CategoryId = category.Id,
            Sum = operation.Sum,
            Comment = operation.Comment,
            PlaceId = placeId,
            DateFrom = operation.DateFrom,
            DateTo = operation.DateTo,
            TimeId = (int)operation.TimeType,
            TimeValue = operation.TimeValue,
        };

        SetRegularTaskRunTime(dbOperation, DateTime.Now.Date);

        await context.RegularOperations.AddAsync(dbOperation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return operationId;
    }

    public async Task UpdateAsync(RegularOperation operation, CancellationToken cancellationToken)
    {
        CheckTime(operation.TimeType, operation.TimeValue);

        Data.Entities.RegularOperation dbOperation = await context.RegularOperations
                                                      .IsUserEntity(environment.UserId, operation.Id)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new NotFoundException("Регулярная операция", operation.Id);

        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);
        int? placeId = await placeService.GetPlaceIdAsync(operation.Place, dbOperation, cancellationToken);

        dbOperation.Sum = operation.Sum;
        dbOperation.Comment = operation.Comment;
        dbOperation.CategoryId = category.Id;
        dbOperation.PlaceId = placeId;
        dbOperation.Name = operation.Name;
        dbOperation.DateFrom = operation.DateFrom;
        dbOperation.DateTo = operation.DateTo;
        dbOperation.TimeId = (int)operation.TimeType;
        dbOperation.TimeValue = operation.TimeValue;

        SetRegularTaskRunTime(dbOperation, DateTime.Now.Date);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.RegularOperation dbOperation = await GetByIdInternal(id, cancellationToken);
        dbOperation.IsDeleted = true;
        await placeService.CheckRemovePlaceAsync(dbOperation.PlaceId, dbOperation.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.RegularOperation dbOperation = await context.RegularOperations
                                                      .IgnoreQueryFilters()
                                                      .Where(x => x.IsDeleted)
                                                      .IsUserEntity(environment.UserId, id)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new NotFoundException("Регулярная операция", id);

        dbOperation.IsDeleted = false;
        await placeService.CheckRestorePlaceAsync(dbOperation.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RunRegularTaskAsync(CancellationToken cancellationToken)
    {
        var dn = DateTime.Now.Date;
        var dbTasks = context.RegularOperations.Where(x => x.RunTime != null && x.RunTime <= dn).ToList();
        foreach (var dbTask in dbTasks)
        {

            var operation = new Models.Operation
            {
                CategoryId = dbTask.CategoryId,
                Comment = dbTask.Comment,
                Sum = dbTask.Sum,
                CreatedTaskId = dbTask.Id,
                PlaceId = dbTask.PlaceId,
                Date = dbTask.DateFrom
            };
            await operationService.CreateAsync(operation, cancellationToken);

            dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, dbTask.RunTime.Value, (RegularTaskTimeTypes)dbTask.TimeId, dbTask.TimeValue);
            context.SaveChanges();
        }
    }

    private async Task<Data.Entities.RegularOperation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.RegularOperation dbOperation = await context.RegularOperations
                                                      .IsUserEntity(environment.UserId, id)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new NotFoundException("Регулярная операция", id);

        return dbOperation;
    }

    private RegularOperation GetBusinessModel(Data.Entities.RegularOperation model, IEnumerable<Place>? dbPlaces = null)
    {
        return new RegularOperation
        {
            CategoryId = model.CategoryId,
            Sum = model.Sum,
            Comment = model.Comment,
            Place = model.PlaceId.HasValue
                ? dbPlaces?.FirstOrDefault(x => x.Id == model.PlaceId)?.Name
                : null,
            Id = model.Id,
            Name = model.Name,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            RunTime = model.RunTime,
            TimeType = (RegularTaskTimeTypes)model.TimeId,
            TimeValue = model.TimeValue,
        };
    }

    private void CheckTime(RegularTaskTimeTypes timeType, int? timeValue)
    {
        if (timeType == RegularTaskTimeTypes.EveryDay)
        {
            if (timeValue != null)
            {
                throw new BusinessException("недопустимое значение интервала");
            }
        }

        if (timeType == RegularTaskTimeTypes.EveryWeek)
        {
            if (timeValue == null || timeValue < 1 || timeValue > 7)
            {
                throw new BusinessException("недопустимое значение интервала");
            }
        }

        if (timeType == RegularTaskTimeTypes.EveryMonth)
        {
            if (timeValue == null || timeValue < 1 || timeValue > 31)
            {
                throw new BusinessException("недопустимое значение интервала");
            }
        }
    }

    private void SetRegularTaskRunTime(Data.Entities.RegularOperation dbTask, DateTime dateNow)
    {
        dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, dateNow, (RegularTaskTimeTypes)dbTask.TimeId, dbTask.TimeValue);
    }

    private DateTime? GetRegularTaskRunTime(DateTime dateFrom, DateTime? dateTo, DateTime dateNow, RegularTaskTimeTypes timeType, int? timeValue)
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
            return GetNextWeekday(date.Date, (DayOfWeek)timeValue);
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

    private DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }
}
