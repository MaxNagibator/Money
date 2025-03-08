using CSharpFunctionalExtensions;
using Money.ApiClient;

namespace Money.Web.Services;

public class CarService(MoneyClient moneyClient, ILogger<CarService> logger)
{
    public async Task<IEnumerable<Car>> GetAllAsync()
    {
        var response = await moneyClient.Cars.Get();

        return response.Content?
                   .Select(x => new Car
                   {
                       Id = x.Id,
                       Name = x.Name,
                   })
               ?? [];
    }

    public async Task<Result> SaveAsync(Car model)
    {
        var request = new CarsClient.SaveRequest
        {
            Name = model.Name,
        };

        try
        {
            if (model.Id == null)
            {
                var response = await moneyClient.Cars.Create(request);
                model.Id = response.Content;
            }
            else
            {
                await moneyClient.Cars.Update(model.Id.Value, request);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ошибка при сохранении категории");
            return Result.Failure("Не удалось сохранить категорию. Пожалуйста, попробуйте еще раз.");
        }

        return Result.Success();
    }
}
