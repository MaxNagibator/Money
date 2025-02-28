using Money.Business;

namespace Money.Api.Definitions;

public class ContainerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RequestEnvironment>();

        builder.Services.AddScoped<AccountsService>();
        builder.Services.AddScoped<AuthService>();

        builder.Services.AddScoped<CategoriesService>();
        builder.Services.AddScoped<DebtsService>();
        builder.Services.AddScoped<OperationsService>();
        builder.Services.AddScoped<UsersService>();
        builder.Services.AddScoped<FastOperationsService>();
        builder.Services.AddScoped<PlacesService>();
        builder.Services.AddScoped<RegularOperationsService>();
        builder.Services.AddScoped<FilesService>();
        builder.Services.AddScoped<CarsService>();
        builder.Services.AddScoped<CarEventsService>();
        builder.Services.AddSingleton<QueueHolder>();
    }
}
