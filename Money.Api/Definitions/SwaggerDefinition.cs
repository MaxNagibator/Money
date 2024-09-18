using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Money.Api.Definitions.Base;

namespace Money.Api.Definitions;

public class SwaggerDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

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

    public override void ConfigureApplication(IApplicationBuilder app)
    {
        //if (app.Environment.IsDevelopment() == false)
        //{
        //    return;
        //}

        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
