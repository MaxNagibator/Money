using Money.Api.Definitions.Base;
using Money.Common;

namespace Money.Api.Definitions;

public class CommonDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new EnumerationConverterFactory());
            });

        services.AddLocalization();
        services.AddHttpContextAccessor();
        services.AddResponseCaching();
        services.AddMemoryCache();
    }

    public override int ApplicationOrderIndex => 2;
    public override void ConfigureApplication(IApplicationBuilder app)
    {
        //app.MapControllers();
        //app.MapDefaultControllerRoute();
    }
}
