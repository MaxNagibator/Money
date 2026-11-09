using Money.ApiClient;

namespace Money.Web.Services;

public class DebtOwnerService(MoneyClient moneyClient)
{
    private const int CacheCapacity = 64;

    private readonly LinkedList<string> _cacheOrder = [];
    private readonly Dictionary<string, string[]> _cache = [];
    private string _lastSearchedValue = string.Empty;

    public async Task<IEnumerable<string>> SearchOwner(string? value, CancellationToken token = default)
    {
        value ??= string.Empty;

        if (_cache.TryGetValue(value, out var cachedResults))
        {
            return cachedResults;
        }

        var diff = value.Length - _lastSearchedValue.Length;

        if (diff > 0
            && value.StartsWith(_lastSearchedValue, StringComparison.OrdinalIgnoreCase)
            && _cache.TryGetValue(_lastSearchedValue, out var cachedOwners)
            && cachedOwners.Length == 0)
        {
            return [];
        }

        var response = await moneyClient.Debts.GetOwners(0, 10, value, token);

        if (response.Content == null)
        {
            return [];
        }

        var owners = response.Content;

        AddToCache(value, owners);
        _lastSearchedValue = value;

        return owners;
    }

    public void InvalidateCache()
    {
        _cache.Clear();
        _cacheOrder.Clear();
        _lastSearchedValue = string.Empty;
    }

    private void AddToCache(string key, string[] value)
    {
        if (_cache.ContainsKey(key))
        {
            _cacheOrder.Remove(key);
        }
        else if (_cache.Count >= CacheCapacity)
        {
            var oldest = _cacheOrder.First;

            if (oldest != null)
            {
                _cache.Remove(oldest.Value);
                _cacheOrder.RemoveFirst();
            }
        }

        _cache[key] = value;
        _cacheOrder.AddLast(key);
    }
}
