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

        var authAuthority = builder.Configuration["AUTH_AUTHORITY"];
        var authClientId = builder.Configuration["AUTH_CLIENT_ID"];

        var githubClientId = builder.Configuration["GITHUB_CLIENT_ID"];
        var githubClientSecret = builder.Configuration["GITHUB_CLIENT_SECRET"];

        if (authAuthority is not null && authClientId is not null
            || githubClientId is not null && githubClientSecret is not null)
        {
            openIddictBuilder
                .AddClient(options =>
                {
                    options.AllowAuthorizationCodeFlow();

                    options.UseAspNetCore()
                        .EnableRedirectionEndpointPassthrough();

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    if (authAuthority is not null && authClientId is not null)
                    {
                        options.AddRegistration(new()
                        {
                            Issuer = new(authAuthority, UriKind.Absolute),
                            ProviderName = "Auth",
                            ProviderDisplayName = "Auth",

                            ClientId = authClientId,
                            Scopes = { "email", "profile", "roles" },

                            RedirectUri = new("/connect/callback", UriKind.Relative),
                        });
                    }

                    if (githubClientId is not null && githubClientSecret is not null)
                    {
                        options.UseWebProviders()
                            .AddGitHub(github =>
                            {
                                github.SetClientId(githubClientId);
                                github.SetClientSecret(githubClientSecret);
                                github.SetRedirectUri(new Uri("/connect/callback", UriKind.Relative));
                                github.AddScopes("read:user", "user:email");
                            });
                    }
                });
        }

        openIddictBuilder.AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });
    }
}
