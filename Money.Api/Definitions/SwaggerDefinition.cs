using System.Reflection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Money.Api.Definitions.Base;

namespace Money.Api.Definitions;

public class SwaggerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Money API",
                Version = "v1",
                Description = "API для управления финансами"
            });

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

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

    public override void ConfigureApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment() == false)
        {
            return;
        }

        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
