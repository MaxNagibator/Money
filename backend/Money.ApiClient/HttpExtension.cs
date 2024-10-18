using System.Collections;

namespace Money.ApiClient;

public static class HttpExtension
{
    public static string ToUriParameters<T>(T? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        Dictionary<string, object?> properties = typeof(T).GetProperties()
            .Where(x => x.GetValue(value) != null)
            .ToDictionary(x => x.Name.ToLower(), x => x.GetValue(value));

        string parameters = string.Join("&", properties.Select(x => $"{x.Key}={GetValue(x.Value)}"));
        return "?" + parameters;
    }

    private static string? GetValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string stringValue => stringValue,
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd"),
            IEnumerable enumerable => string.Join(",", enumerable.Cast<object>().Select(x => x.ToString())),
            var _ => value.ToString(),
        };
    }
}
