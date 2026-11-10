using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using System.Threading.RateLimiting;

namespace Money.Api.Definitions;

public class AuthorizationDefinition : AppDefinition
{
    public const string SensitiveAuthPolicy = "SensitiveAuth";

    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                policyBuilder => policyBuilder
                    .WithOrigins(builder.Configuration["CORS_ORIGIN"] ?? "https://localhost:7168")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(SensitiveAuthPolicy, httpContext =>
            {
                var openIdRequest = httpContext.GetOpenIddictServerRequest();

                if (openIdRequest?.IsRefreshTokenGrantType() == true)
                {
                    return RateLimitPartition.GetNoLimiter("refresh-token");
                }

                var partitionKey = ResolveClientKey(httpContext);

                if (partitionKey == null)
                {
                    return RateLimitPartition.GetNoLimiter("in-memory");
                }

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
                    _ => new()
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    });
            });
        });
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseCors("AllowSpecificOrigin");

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();
    }

    private static string? ResolveClientKey(HttpContext httpContext)
    {
        if (httpContext.Connection.RemoteIpAddress != null)
        {
            return httpContext.Connection.RemoteIpAddress.ToString();
        }

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(forwarded))
        {
            return null;
        }

        return forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
    }
}
