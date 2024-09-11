using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace Money.Api.Definitions;

public static class SwaggerDefinition
{
    public static void AddSwaggerDefinition(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.ResolveConflictingActions(descriptions => descriptions.First());

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "OAuth2.0 Authorization",
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri("/connect/token", UriKind.Relative)
                    }
                },
                In = ParameterLocation.Header,
                Name = HeaderNames.Authorization,
                Type = SecuritySchemeType.OAuth2
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "oauth2",
                            Type = ReferenceType.SecurityScheme
                        },
                        In = ParameterLocation.Cookie,
                        Type = SecuritySchemeType.OAuth2
                    },
                    new List<string>()
                }
            });
        });
    }

    public static void UseSwaggerDefinition(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
