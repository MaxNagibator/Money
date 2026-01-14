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
            options.EnableSensitiveDataLogging();
            options.UseLoggerFactory(LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
            }));
            options.LogTo((x) => {
                System.IO.File.AppendAllText("E:\\bobgroup\\projects\\money\\repo\\money\\backend\\Money.Api\\bin\\Debug\\net8.0\\logs\\2026-01-14\\1.txt",
                x);
            });
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
