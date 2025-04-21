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
        var automigrate = app.Configuration["AUTO_MIGRATE"];
        if (automigrate?.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "true" || automigrate == "1")
        {
            app.Services.InitializeDatabaseContext();
        }
    }
}
