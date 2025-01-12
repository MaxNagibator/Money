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
        var dbOperations = context.RegularOperations
            .IsUserEntity(environment.UserId);

        var placeIds = await dbOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        var places = await placeService.GetPlacesAsync(placeIds, cancellationToken);

        var operations = await dbOperations
            .OrderBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return operations.Select(x => GetBusinessModel(x, places)).ToList();
    }

    public async Task<RegularOperation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var dbOperation = await GetByIdInternal(id, cancellationToken);

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

        var category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);

        var operationId = await userService.GetNextRegularOperationIdAsync(cancellationToken);

        var placeId = await placeService.GetPlaceIdAsync(operation.Place, cancellationToken);

        var dbOperation = new Data.Entities.RegularOperation
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
            TimeTypeId = (int)operation.TimeType,
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

        var dbOperation = await context.RegularOperations
                              .IsUserEntity(environment.UserId, operation.Id)
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException("Регулярная операция", operation.Id);

        var category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);
        var placeId = await placeService.GetPlaceIdAsync(operation.Place, dbOperation, cancellationToken);

        dbOperation.Sum = operation.Sum;
        dbOperation.Comment = operation.Comment;
        dbOperation.CategoryId = category.Id;
        dbOperation.PlaceId = placeId;
        dbOperation.Name = operation.Name;
        dbOperation.DateFrom = operation.DateFrom;
        dbOperation.DateTo = operation.DateTo;
        dbOperation.TimeTypeId = (int)operation.TimeType;
        dbOperation.TimeValue = operation.TimeValue;

        SetRegularTaskRunTime(dbOperation, DateTime.Now.Date);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var dbOperation = await GetByIdInternal(id, cancellationToken);
        dbOperation.IsDeleted = true;
        await placeService.CheckRemovePlaceAsync(dbOperation.PlaceId, dbOperation.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var dbOperation = await context.RegularOperations
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
        var dateNow = DateTime.Now.Date;

        var dbTasks = await context.RegularOperations
            .Where(x => x.RunTime != null && x.RunTime <= dateNow)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var dbTask in dbTasks)
        {
            try
            {
                var operation = new Operation
                {
                    CategoryId = dbTask.CategoryId,
                    Comment = dbTask.Comment,
                    Sum = dbTask.Sum,
                    CreatedTaskId = dbTask.Id,
                    PlaceId = dbTask.PlaceId,
                    Date = dbTask.DateFrom,
                };

                // TODO: это гавнина
                environment.UserId = dbTask.UserId;
                await operationService.CreateAsync(operation, cancellationToken);

                dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, dbTask.RunTime!.Value, (RegularOperationTimeTypes)dbTask.TimeTypeId, dbTask.TimeValue);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task<Data.Entities.RegularOperation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        var dbOperation = await context.RegularOperations
                              .IsUserEntity(environment.UserId, id)
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException("регулярная операция", id);

        return dbOperation;
    }

    private RegularOperation GetBusinessModel(Data.Entities.RegularOperation model, IEnumerable<Place>? dbPlaces = null)
    {
        return new()
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
            TimeType = (RegularOperationTimeTypes)model.TimeTypeId,
            TimeValue = model.TimeValue,
        };
    }

    private void CheckTime(RegularOperationTimeTypes timeType, int? timeValue)
    {
        switch (timeType)
        {
            case RegularOperationTimeTypes.EveryDay when timeValue != null:
                throw new("Извините, но значение интервала должно отсутствовать при Каждый день");

            case RegularOperationTimeTypes.EveryWeek when timeValue is null or < 1 or > 7:
                throw new("Извините, но значение интервала должно быть в диапазоне от 1 до 7 при Каждую неделю");

            case RegularOperationTimeTypes.EveryMonth when timeValue is null or < 1 or > 31:
                throw new("Извините, но значение интервала должно быть в диапазоне от 1 до 31 при Каждый месяц");

            default:
                return;
        }
    }

    private void SetRegularTaskRunTime(Data.Entities.RegularOperation dbTask, DateTime dateNow)
    {
        dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, dateNow, (RegularOperationTimeTypes)dbTask.TimeTypeId, dbTask.TimeValue);
    }

    private DateTime? GetRegularTaskRunTime(DateTime dateFrom, DateTime? dateTo, DateTime dateNow, RegularOperationTimeTypes timeType, int? timeValue)
    {
        var date = dateFrom;

        if (dateNow > date)
        {
            date = dateNow;
        }
        else if (dateTo < dateNow)
        {
            return null;
        }

        switch (timeType)
        {
            case RegularOperationTimeTypes.EveryDay:
                date = date.AddDays(1);
                return date.Date;

            case RegularOperationTimeTypes.EveryWeek:
                date = date.AddDays(1);

                if (timeValue == 7)
                {
                    timeValue = 0;
                }

                if (timeValue == null)
                {
                    throw new BusinessException("Извините");
                }

                return GetNextWeekday(date.Date, (DayOfWeek)timeValue);

            case RegularOperationTimeTypes.EveryMonth:
                var dt = new DateTime(date.Year, date.Month, 1);

                if (dt.Day >= timeValue)
                {
                    dt = dt.AddMonths(1);
                }

                if (timeValue == null)
                {
                    throw new BusinessException("Извините");
                }

                var nextDt = dt.AddDays(timeValue.Value - 1);

                dt = dt.Month < nextDt.Month
                    ? dt.AddMonths(1).AddDays(-1)
                    : nextDt;

                return dt;

            default:
                throw new ArgumentOutOfRangeException(nameof(timeType), timeType, null);
        }
    }

    private DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }
}
