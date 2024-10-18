namespace Money.Api.Definitions;

public class AuthorizationDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                policyBuilder => policyBuilder
                    .WithOrigins("https://localhost:7168")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseCors("AllowSpecificOrigin");

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
