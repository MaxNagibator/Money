using Money.Api.Definitions.Base;
using NLog;
using NLog.Web;

Logger? logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

logger.Debug("init main");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    var startup = new Startup(builder.Configuration);
    startup.ConfigureServices(builder.Services);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();



    //builder.AddDefinitions(,typeof(Program));
    //builder.Services.AddDefinitions(typeof(Program));


    WebApplication app = builder.Build();
    startup.Configure(app, app.Environment);//, builder.Services);

    //app.UseDefinitions();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}

public class Startup
{
    private IServiceCollection _services;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDefinitions(Configuration, typeof(Program));
        _services = services;
    }

    // Use this method to configure the HTTP request pipeline.  
    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseDefinitions(_services);
    }
}

public partial class Program { }
