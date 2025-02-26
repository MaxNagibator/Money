using Money.Data.Extensions;

namespace Money.Business.Services;

public class CarService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public async Task<IEnumerable<Car>> GetAsync(CancellationToken cancellationToken)
    {
        var models = await context.Cars
            .IsUserEntity(environment.UserId)
            .OrderBy(x => x.Id)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return models;
    }

    public async Task<Car> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(id, cancellationToken);
        return GetBusinessModel(entity);
    }

    public async Task<int> CreateAsync(Car model, CancellationToken cancellationToken)
    {
        var id = await userService.GetNextCarIdAsync(cancellationToken);

        var entity = new Data.Entities.Car
        {
            Id = id,
            UserId = environment.UserId,
            Name = model.Name,
        };

        await context.Cars.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(Car model, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternal(model.Id, cancellationToken);
        entity.Name = model.Name;

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

    private static Car GetBusinessModel(Data.Entities.Car model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }

    private async Task<Data.Entities.Car> GetByIdInternal(int id, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        var query = context.Cars
            .IsUserEntity(environment.UserId, id);

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(cancellationToken)
               ?? throw new NotFoundException("авто", id);
    }
}
