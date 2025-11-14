using Money.ApiClient;

namespace Money.Web.Services;

public class PlaceService(MoneyClient moneyClient)
{
    private static readonly Dictionary<string, string[]> Cache = [];
    private string _lastSearchedValue = string.Empty;

    public async Task<IEnumerable<string>> SearchPlace(string? value, CancellationToken token = default)
    {
        value ??= string.Empty;

        if (Cache.TryGetValue(value, out var cachedResults))
        {
            return cachedResults;
        }

        var diff = value.Length - _lastSearchedValue.Length;

        if (diff > 0
            && value[..^diff] == _lastSearchedValue
            && Cache.TryGetValue(_lastSearchedValue, out var cachedPlaces)
            && cachedPlaces.Length == 0)
        {
            return [];
        }

        var response = await moneyClient.Operations.GetPlaces(0, 10, value, token);

        if (response.Content == null)
        {
            return [];
        }

        var places = response.Content;

        Cache[value] = places;
        _lastSearchedValue = value;

        return places;
    }
}
