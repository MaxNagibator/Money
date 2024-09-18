using Microsoft.EntityFrameworkCore;
using Money.Api.Definitions.Base;
using Money.Data;

namespace Money.Api.Definitions;

public class DbContextDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(nameof(ApplicationDbContext)));
            options.UseSnakeCaseNamingConvention();
            options.UseOpenIddict();
        });
    }
}
