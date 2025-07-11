using Money.Api.Configuration;
using Money.Data;

namespace Money.Api.Definitions;

public class OpenIddictDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection(nameof(TokenSettings)));

        var tokenSettings = builder.Configuration.GetSection(nameof(TokenSettings)).Get<TokenSettings>()
                            ?? new TokenSettings();

        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    .SetVerificationEndpointUris("connect/verify")
                    .SetUserinfoEndpointUris("connect/userinfo")
                    .SetCryptographyEndpointUris("connect/jwks");

                options.AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                options.SetAccessTokenLifetime(tokenSettings.AccessTokenLifetime);
                options.SetRefreshTokenLifetime(tokenSettings.RefreshTokenLifetime);

                options.AcceptAnonymousClients();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }
}
