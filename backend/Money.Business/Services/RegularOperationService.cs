using Microsoft.Extensions.Logging;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class RegularOperationService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService,
    CategoryService categoryService,
    PlaceService placeService,
    OperationService operationService,
    ILogger<RegularOperationService> logger)
{
    public async Task<IEnumerable<RegularOperation>> GetAsync(CancellationToken cancellationToken)
    {
        var entities = context.RegularOperations
            .IsUserEntity(environment.UserId);

        var placeIds = await entities
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        var places = await placeService.GetPlacesAsync(placeIds, cancellationToken);

        var models = await entities
            .OrderBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return models.Select(x => GetBusinessModel(x, places)).ToList();
    }

    public async Task<RegularOperation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(id, cancellationToken);

        List<Place>? places = null;

        if (entity.PlaceId != null)
        {
            places = await placeService.GetPlacesAsync([entity.PlaceId.Value], cancellationToken);
        }

        return GetBusinessModel(entity, places);
    }

    public async Task<int> CreateAsync(RegularOperation model, CancellationToken cancellationToken)
    {
        CheckTime(model.TimeType, model.TimeValue);

        var category = await categoryService.GetByIdAsync(model.CategoryId, cancellationToken);
        var operationId = await userService.GetNextRegularOperationIdAsync(cancellationToken);
        var placeId = await placeService.GetPlaceIdAsync(model.Place, cancellationToken);

        var entity = new Data.Entities.RegularOperation
        {
            Id = operationId,
            Name = model.Name,
            UserId = environment.UserId,
            CategoryId = category.Id,
            Sum = model.Sum,
            Comment = model.Comment,
            PlaceId = placeId,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            TimeTypeId = (int)model.TimeType,
            TimeValue = model.TimeValue,
        };

        SetRegularTaskRunTime(entity, DateTime.Now.Date);

        await context.RegularOperations.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return operationId;
    }

    public async Task UpdateAsync(RegularOperation model, CancellationToken cancellationToken)
    {
        CheckTime(model.TimeType, model.TimeValue);

        var entity = await context.RegularOperations
                         .IsUserEntity(environment.UserId, model.Id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException("Регулярная операция", model.Id);

        var category = await categoryService.GetByIdAsync(model.CategoryId, cancellationToken);
        var placeId = await placeService.GetPlaceIdAsync(model.Place, entity, cancellationToken);

        entity.Sum = model.Sum;
        entity.Comment = model.Comment;
        entity.CategoryId = category.Id;
        entity.PlaceId = placeId;
        entity.Name = model.Name;
        entity.DateFrom = model.DateFrom;
        entity.DateTo = model.DateTo;
        entity.TimeTypeId = (int)model.TimeType;
        entity.TimeValue = model.TimeValue;

        SetRegularTaskRunTime(entity, DateTime.Now.Date);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(id, cancellationToken);
        entity.IsDeleted = true;
        await placeService.CheckRemovePlaceAsync(entity.PlaceId, entity.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await context.RegularOperations
                         .IgnoreQueryFilters()
                         .Where(x => x.IsDeleted)
                         .IsUserEntity(environment.UserId, id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException("Регулярная операция", id);

        entity.IsDeleted = false;
        await placeService.CheckRestorePlaceAsync(entity.PlaceId, cancellationToken);
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
                var model = new Operation
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
                await operationService.CreateAsync(model, cancellationToken);

                dbTask.RunTime = GetRegularTaskRunTime(dbTask.DateFrom, dbTask.DateTo, dbTask.RunTime!.Value, (RegularOperationTimeTypes)dbTask.TimeTypeId, dbTask.TimeValue);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при запуске регулярной операции ({UserId},{Id})", dbTask.UserId, dbTask.Id);
            }
        }
    }

    private async Task<Data.Entities.RegularOperation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        var entity = await context.RegularOperations
                         .IsUserEntity(environment.UserId, id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException("регулярная операция", id);

        return entity;
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
                throw new BusinessException("Извините, но значение интервала должно отсутствовать при Каждый день");

            case RegularOperationTimeTypes.EveryWeek when timeValue is null or < 1 or > 7:
                throw new BusinessException("Извините, но значение интервала должно быть в диапазоне от 1 до 7 при Каждую неделю");

            case RegularOperationTimeTypes.EveryMonth when timeValue is null or < 1 or > 31:
                throw new BusinessException("Извините, но значение интервала должно быть в диапазоне от 1 до 31 при Каждый месяц");

            case RegularOperationTimeTypes.EveryDay:
            case RegularOperationTimeTypes.EveryWeek:
            case RegularOperationTimeTypes.EveryMonth:
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

                if (dateNow.Day >= timeValue)
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
