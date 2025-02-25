using Money.Data.Extensions;

namespace Money.Business.Services;

public class CarService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public async Task<IEnumerable<Car>> GetAsync(CancellationToken cancellationToken = default)
    {
        var query = context.Cars.IsUserEntity(environment.UserId);
        var cars = await query
            .OrderBy(x => x.Id)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return cars;
    }

    private Car GetBusinessModel(Data.Entities.Car model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }

    public async Task<Car> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var dbCar = await GetByIdInternal(id, cancellationToken);
        return GetBusinessModel(dbCar);
    }

    public async Task<int> CreateAsync(Car car, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        var carId = await userService.GetNextCarIdAsync(cancellationToken);

        var dbCar = new Data.Entities.Car
        {
            Id = carId,
            UserId = environment.UserId.Value,
            Name = car.Name,
        };

        await context.Cars.AddAsync(dbCar, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return carId;
    }

    public async Task UpdateAsync(Car car, CancellationToken cancellationToken)
    {
        var dbCar = await GetByIdInternal(car.Id, cancellationToken);
        dbCar.Name = car.Name;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbCar = await GetByIdInternal(id, cancellationToken);

        dbCar.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbCar = await GetCar(id);

        if (dbCar.IsDeleted == false)
        {
            throw new BusinessException("Извините, но невозможно восстановить неудаленную сущность");
        }

        dbCar.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        async Task<Data.Entities.Car> GetCar(int CarId)
        {
            var domainCar = await context.Cars
                .IgnoreQueryFilters()
                .IsUserEntity(environment.UserId, CarId)
                .FirstOrDefaultAsync(cancellationToken);

            if (domainCar == null)
            {
                throw new NotFoundException("авто", CarId);
            }

            return domainCar;
        }
    }

    private async Task<Data.Entities.Car> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        var dbCar = await context.Cars
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("авто", id);

        return dbCar;
    }

    public async Task<IEnumerable<CarEvent>> GetEventsAsync(int carId, CancellationToken cancellationToken = default)
    {
        var query = context.CarEvents.IsUserEntity(environment.UserId);

        var carEvents = await query
            .Where(x => x.CarId == carId)
            .OrderBy(x => x.Id)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return carEvents;
    }

    private CarEvent GetBusinessModel(Data.Entities.CarEvent model)
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

    public async Task<CarEvent> GetEventByIdAsync(int id, CancellationToken cancellationToken)
    {
        var dbCarEvent = await GetCarEventByIdInternal(id, cancellationToken);
        return GetBusinessModel(dbCarEvent);
    }

    public async Task<int> CreateAsync(CarEvent carEvent, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        var carId = await userService.GetNextCarIdAsync(cancellationToken);

        var dbCarEvent = new Data.Entities.CarEvent
        {
            Id = carId,
            UserId = environment.UserId.Value,
            TypeId = (int)carEvent.Type,
            Title = carEvent.Title,
            Date = carEvent.Date,
            Mileage = carEvent.Mileage,
            Comment = carEvent.Comment,
        };

        await context.CarEvents.AddAsync(dbCarEvent, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return carId;
    }

    public async Task UpdateAsync(CarEvent CarEvent, CancellationToken cancellationToken)
    {
        var dbCar = await GetCarEventByIdInternal(CarEvent.Id, cancellationToken);
        dbCar.TypeId = (int)CarEvent.Type;
        dbCar.Title = CarEvent.Title;
        dbCar.Date = CarEvent.Date;
        dbCar.Mileage = CarEvent.Mileage;
        dbCar.Comment = CarEvent.Comment;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEventAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbCarEvent = await GetCarEventByIdInternal(id, cancellationToken);

        dbCarEvent.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreEventAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbCarEvent = await GetCarEvent(id);

        if (dbCarEvent.IsDeleted == false)
        {
            throw new BusinessException("Извините, но невозможно восстановить неудаленную сущность");
        }

        dbCarEvent.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        async Task<Data.Entities.CarEvent> GetCarEvent(int CarId)
        {
            var dbCarEvent = await context.CarEvents
                .IgnoreQueryFilters()
                .IsUserEntity(environment.UserId, CarId)
                .FirstOrDefaultAsync(cancellationToken);

            if (dbCarEvent == null)
            {
                throw new NotFoundException("авто", CarId);
            }

            return dbCarEvent;
        }
    }

    private async Task<Data.Entities.CarEvent> GetCarEventByIdInternal(int id, CancellationToken cancellationToken)
    {
        var dbCar = await context.CarEvents
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("авто", id);

        return dbCar;
    }
}
