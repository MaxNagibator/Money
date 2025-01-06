using System.Net.Http.Json;
using System.Security.Claims;

namespace Money.Web.Services.Authentication;

public class JwtParser(HttpClient client)
{
    public async Task<ClaimsPrincipal?> ValidateJwt(string token)
    {
        var claimsDictionary = await ParseJwt(token);

        if (claimsDictionary == null)
        {
            return null;
        }

        List<Claim> claims = [];

        foreach ((var key, var value) in claimsDictionary)
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
        var request = new HttpRequestMessage(HttpMethod.Get, "connect/userinfo");
        request.Headers.Authorization = new("Bearer", accessToken);

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode == false)
        {
            return null;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return userInfo ?? throw new InvalidOperationException();
    }
}
