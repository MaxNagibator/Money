using Money.CoreLib;
using NLog;
using NLog.Web;

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.AddServiceDefaults();
    builder.AddDefinitions(typeof(Program));

    var app = builder.Build();

    app.UseDefinitions();
    app.MapDefaultEndpoints();
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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

public partial class Program;
