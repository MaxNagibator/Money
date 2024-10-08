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

        var properties = typeof(T).GetProperties().Where(x => x.GetValue(value) != null)
            .ToDictionary(x => x.Name.ToLower(), x => x.GetValue(value));
        var parameters = string.Join("&", properties.Select(x => $"{x.Key}={GetValue(x.Value)}"));
        return "?" + parameters;
    }
    private static string? GetValue(object? value)
    {
        switch (value)
        {
            case null:
                return string.Empty;
            case string stringValue:
                return stringValue;
            case DateTime dateTime:
                return dateTime.ToString("yyyy-MM-dd");
            case IEnumerable enumerable:
                return string.Join(",", enumerable.Cast<object>().Select(x => x.ToString()));
            default:
                return value.ToString();
        }
    }
}