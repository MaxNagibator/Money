using Money.ApiClient;

namespace Money.Web.Services;

public class PlaceService(MoneyClient moneyClient)
{
    private static readonly Dictionary<string, string[]> Cache = new();
    private string _lastSearchedValue = string.Empty;

    public async Task<IEnumerable<string>> SearchPlace(string value, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        if (Cache.TryGetValue(value, out string[]? cachedResults))
        {
            return EnsureValueInList(cachedResults, value);
        }

        int diff = value.Length - _lastSearchedValue.Length;

        if (diff > 0 && value[..^diff] == _lastSearchedValue)
        {
            if (Cache.TryGetValue(_lastSearchedValue, out string[]? cachedPlaces))
            {
                if (cachedPlaces.Length == 0)
                {
                    return [value];
                }
            }
        }

        ApiClientResponse<string[]> response = await moneyClient.Payment.GetPlaces(0, 10, value, token);

        if (response.Content == null)
        {
            return [value];
        }

        string[]? places = response.Content;

        Cache[value] = places;
        _lastSearchedValue = value;

        return EnsureValueInList(places, value);
    }

    private static List<T> EnsureValueInList<T>(IEnumerable<T> list, T value)
    {
        List<T> newList = [..list];

        if (newList.Count == 0 || newList.All(x => !Equals(x, value)))
        {
            newList.Insert(0, value);
        }

        return newList;
    }
}
