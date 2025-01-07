using Microsoft.EntityFrameworkCore;
using Money.Data;

namespace Money.Api.Definitions;

public class DbContextDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ApplicationDbContext)));
            options.UseSnakeCaseNamingConvention();
            options.UseOpenIddict();
        });
    }

    public override void ConfigureApplication(WebApplication app)
    {
        if (app.Configuration["AUTO_MIGRATE"] == "true")
        {
            app.Services.InitializeDatabaseContext();
        }
    }
}
