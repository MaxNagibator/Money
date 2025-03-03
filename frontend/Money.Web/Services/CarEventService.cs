using Money.ApiClient;

namespace Money.Web.Services;

public class CarEventService(MoneyClient moneyClient)
{
    public async Task<IEnumerable<CarEvent>> GetAllAsync(int carId)
    {
        var response = await moneyClient.CarEvents.Get(carId);

        return response.Content?
                   .Select(x => new CarEvent
                   {
                       Id = x.Id,
                       Type = CarEventTypes.Values[x.TypeId],
                       Title = x.Title,
                       Comment = x.Comment,
                       Mileage = x.Mileage,
                       Date = x.Date,
                   })
               ?? [];
    }
}
