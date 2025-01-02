using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Money.Api.Definitions;

public class SwaggerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Money API",
                Version = "v1",
                Description = "API для управления финансами",
            });

            options.CustomSchemaIds(x => x.FullName);

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.ResolveConflictingActions(descriptions => descriptions.First());

            options.AddSecurityDefinition("oauth2", new()
            {
                Description = "OAuth2.0 Authorization",
                Flows = new()
                {
                    Password = new()
                    {
                        TokenUrl = new("/connect/token", UriKind.Relative),
                    },
                },
                In = ParameterLocation.Header,
                Name = HeaderNames.Authorization,
                Type = SecuritySchemeType.OAuth2,
            });

            options.AddSecurityRequirement(new()
            {
                {
                    new()
                    {
                        Reference = new()
                        {
                            Id = "oauth2",
                            Type = ReferenceType.SecurityScheme,
                        },
                        In = ParameterLocation.Cookie,
                        Type = SecuritySchemeType.OAuth2,
                    },
                    new List<string>()
                },
            });
        });
    }

    public override void ConfigureApplication(WebApplication app)
    {
        var swaggerForEveryOneHome = true;

        if (!swaggerForEveryOneHome && app.Environment.IsDevelopment() == false)
        {
            return;
        }

        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
