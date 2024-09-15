using Money.Api.Definitions.Base;
using Money.Api.Middlewares;
using NLog;
using NLog.Web;

Logger? logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

logger.Debug("init main");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.AddDefinitions(typeof(Program));
    WebApplication app = builder.Build();

    app.UseDefinitions();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
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
