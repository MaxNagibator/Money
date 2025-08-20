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

        var openIddictBuilder = builder.Services.AddOpenIddict()
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
                    .AllowRefreshTokenFlow()
                    .AllowAuthorizationCodeFlow()
                    .AllowCustomFlow("external")
                    .RequireProofKeyForCodeExchange();

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
            });

        if (builder.Configuration["GITHUB_CLIENT_ID"] is not null && builder.Configuration["GITHUB_CLIENT_SECRET"] is not null)
        {
            openIddictBuilder
                .AddClient(options =>
                {
                    options.AllowAuthorizationCodeFlow();

                    options.UseAspNetCore()
                        .EnableRedirectionEndpointPassthrough();

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    options.UseWebProviders()
                        .AddGitHub(github =>
                        {
                            github.SetClientId(builder.Configuration["GITHUB_CLIENT_ID"] ?? string.Empty);
                            github.SetClientSecret(builder.Configuration["GITHUB_CLIENT_SECRET"] ?? string.Empty);
                            github.SetRedirectUri(new Uri("/connect/callback", UriKind.Relative));
                            github.AddScopes("read:user", "user:email");
                        });
                });
        }

        openIddictBuilder.AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });
    }
}
