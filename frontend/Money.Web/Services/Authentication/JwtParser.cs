using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Money.Web.Services.Authentication;

public class JwtParser(HttpClient client)
{
    public static long? TryReadExp(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var parts = token.Split('.');

        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            var payload = parts[1];
            var padding = (4 - payload.Length % 4) % 4;
            var base64 = payload.Replace('-', '+').Replace('_', '/') + new string('=', padding);
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("exp", out var exp) && exp.ValueKind == JsonValueKind.Number)
            {
                return exp.GetInt64();
            }

            return null;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }

    public async Task<ClaimsPrincipal?> ValidateJwt(string token)
    {
        var claimsDictionary = await ParseJwt(token);

        if (claimsDictionary == null)
        {
            return null;
        }

        List<Claim> claims = [];

        foreach (var (key, value) in claimsDictionary)
        {
            var claimType = key switch
            {
                "sub" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                "name" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                "email" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                _ => key,
            };

            claims.Add(new(claimType, value.ToString() ?? string.Empty));
        }

        var claimsIdentity = new ClaimsIdentity(claims, "jwt");

        return new(claimsIdentity);
    }

    private async Task<Dictionary<string, object>?> ParseJwt(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "connect/userinfo");
        request.Headers.Authorization = new("Bearer", accessToken);
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return userInfo ?? throw new InvalidOperationException();
    }
}
