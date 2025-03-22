using Money.Data.Extensions;

namespace Money.Business.Services;

public class CarEventsService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    CarsService carsService,
    UsersService usersService)
{
    public async Task<IEnumerable<CarEvent>> GetAsync(int carId, CancellationToken cancellationToken = default)
    {
        var models = await context.CarEvents
            .IsUserEntity(environment.UserId)
            .Where(x => x.CarId == carId)
            .OrderBy(x => x.Id)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return models;
    }

    public async Task<CarEvent> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken: cancellationToken);
        return GetBusinessModel(entity);
    }

    public async Task<int> CreateAsync(CarEvent model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var car = await carsService.GetByIdAsync(model.CarId, cancellationToken);
        var id = await usersService.GetNextCarEventIdAsync(cancellationToken);

        var entity = new Data.Entities.CarEvent
        {
            Id = id,
            UserId = environment.UserId,
            CarId = car.Id,
            TypeId = (int)model.Type,
            Title = model.Title,
            Date = model.Date,
            Mileage = model.Mileage,
            Comment = model.Comment,
        };

        await context.CarEvents.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(CarEvent model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var car = await carsService.GetByIdAsync(model.CarId, cancellationToken);
        var entity = await GetByIdInternal(model.Id, cancellationToken: cancellationToken);

        entity.CarId = car.Id;
        entity.TypeId = (int)model.Type;
        entity.Title = model.Title;
        entity.Date = model.Date;
        entity.Mileage = model.Mileage;
        entity.Comment = model.Comment;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken: cancellationToken);
        entity.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, true, cancellationToken);

        if (entity.IsDeleted == false)
        {
            throw new BusinessException("Извините, но невозможно восстановить неудаленную сущность");
        }

        entity.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static CarEvent GetBusinessModel(Data.Entities.CarEvent model)
    {
        return new()
        {
            Id = model.Id,
            CarId = model.CarId,
            Type = (CarEventTypes)model.TypeId,
            Title = model.Title,
            Date = model.Date,
            Mileage = model.Mileage,
            Comment = model.Comment,
        };
    }

    private static void Validate(CarEvent model)
    {
        if (Enum.IsDefined(model.Type) == false)
        {
            throw new BusinessException("Извините, неподдерживаемый тип события");
        }

        if (model.Title?.Length > 1000)
        {
            throw new BusinessException("Извините, заголовок слишком длинный");
        }

        if (model.Mileage < 0)
        {
            throw new BusinessException("Извините, пробег должен быть положительным");
        }
    }

    private async Task<Data.Entities.CarEvent> GetByIdInternal(int id, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
    {
        var query = context.CarEvents
            .IsUserEntity(environment.UserId, id);

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(cancellationToken)
               ?? throw new NotFoundException("авто-событие", id);
    }
}
