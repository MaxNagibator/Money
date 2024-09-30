using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Money.Web.Services;

public class JwtParser(IHttpClientFactory clientFactory)
{
    private readonly HttpClient _client = clientFactory.CreateClient("api_base");

    public async Task<ClaimsPrincipal?> ValidateJwt(string token)
    {
        Dictionary<string, object>? claimsDictionary = await GetUserInfo(token);

        if (claimsDictionary == null)
        {
            return null;
        }

        List<Claim> claims = [];

        foreach ((string? key, object? value) in claimsDictionary)
        {
            string claimType = key switch
            {
                "sub" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                "name" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                "email" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                var _ => key,
            };

            claims.Add(new Claim(claimType, value.ToString() ?? string.Empty));
        }

        ClaimsIdentity claimsIdentity = new(claims, "jwt");

        return new ClaimsPrincipal(claimsIdentity);
    }

    private async Task<Dictionary<string, object>?> GetUserInfo(string accessToken)
    {
        HttpRequestMessage request = new(HttpMethod.Get, "connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await _client.SendAsync(request);

        if (response.IsSuccessStatusCode == false)
        {
            return null;
        }

        // todo если тут вернулся 401 Unauthorized, нужно разлогинить человечка.
        response.EnsureSuccessStatusCode();

        Dictionary<string, object>? userInfo = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return userInfo ?? throw new InvalidOperationException();
    }
}
