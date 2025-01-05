namespace Money.Api.Extensions;

public static class ParseExtensions
{
    public static List<int>? ParseIds(this string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return null;
        }

        return ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => int.TryParse(id, out var parsedId) ? (int?)parsedId : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }
}
