using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Money.Api.Data;
using Money.Api.Definitions;
using Money.Api.Services;
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

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("SecurityDb"));
        options.UseSnakeCaseNamingConvention();
        options.UseOpenIddict();
    });

    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerDefinition();
    builder.Services.AddOpenIddictDefinition();
    builder.Services.AddScoped<AccountService>();
    builder.Services.AddScoped<AuthorizationService>();

    builder.Services.AddCors();

    WebApplication app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDefinition();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapDefaultControllerRoute();

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
