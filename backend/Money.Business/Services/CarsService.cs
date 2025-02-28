using Money.Data.Extensions;

namespace Money.Business.Services;

public class CarsService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UsersService usersService)
{
    public async Task<IEnumerable<Car>> GetAsync(CancellationToken cancellationToken = default)
    {
        var models = await context.Cars
            .IsUserEntity(environment.UserId)
            .OrderBy(x => x.Id)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return models;
    }

    public async Task<Car> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken: cancellationToken);
        return GetBusinessModel(entity);
    }

    public async Task<int> CreateAsync(Car model, CancellationToken cancellationToken = default)
    {
        var id = await usersService.GetNextCarIdAsync(cancellationToken);

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

    public async Task UpdateAsync(Car model, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(model.Id, cancellationToken: cancellationToken);
        entity.Name = model.Name;

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

    private static Car GetBusinessModel(Data.Entities.Car model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }

    private async Task<Data.Entities.Car> GetByIdInternal(int id, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
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
