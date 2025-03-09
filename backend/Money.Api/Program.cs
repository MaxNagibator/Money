#pragma warning disable S2139
using Money.CoreLib;
using NLog;
using NLog.Web;
using System.Globalization;

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

    var culture = new CultureInfo("ru-RU");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    await app.RunAsync();
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

public partial class Program
{
    protected Program()
    {
    }
}
