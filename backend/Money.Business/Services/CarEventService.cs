using Money.Data.Extensions;

namespace Money.Business.Services;

public class CarEventService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public async Task<IEnumerable<CarEvent>> GetAsync(int carId, CancellationToken cancellationToken)
    {
        var models = await context.CarEvents
            .IsUserEntity(environment.UserId)
            .Where(x => x.CarId == carId)
            .OrderBy(x => x.Id)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return models;
    }

    public async Task<CarEvent> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(id, cancellationToken);
        return GetBusinessModel(entity);
    }

    public async Task<int> CreateAsync(CarEvent model, CancellationToken cancellationToken)
    {
        var id = await userService.GetNextCarEventIdAsync(cancellationToken);

        var entity = new Data.Entities.CarEvent
        {
            Id = id,
            UserId = environment.UserId!.Value,
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

    public async Task UpdateAsync(CarEvent model, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(model.Id, cancellationToken);
        entity.TypeId = (int)model.Type;
        entity.Title = model.Title;
        entity.Date = model.Date;
        entity.Mileage = model.Mileage;
        entity.Comment = model.Comment;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(id, cancellationToken);
        entity.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(id, cancellationToken, true);

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
            Type = (CarEventTypes)model.TypeId,
            Title = model.Title,
            Date = model.Date,
            Mileage = model.Mileage,
            Comment = model.Comment,
        };
    }

    private async Task<Data.Entities.CarEvent> GetByIdInternal(int id, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        var query = context.CarEvents
            .IsUserEntity(environment.UserId, id);

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(cancellationToken)
               ?? throw new NotFoundException("авто", id);
    }
}
