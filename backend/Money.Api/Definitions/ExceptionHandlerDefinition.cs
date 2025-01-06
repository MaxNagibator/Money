using Microsoft.AspNetCore.Http.Features;
using Money.Api.Middlewares;

namespace Money.Api.Definitions;

public class ExceptionHandlerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        builder.Services.AddExceptionHandler<ExceptionHandler>();
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
    }
}
