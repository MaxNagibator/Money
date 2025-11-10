using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.ServiceDiscovery;
using Money.ApiClient;
using Money.Web.Services.Authentication;
using MudBlazor.Services;
using MudBlazor.Translations;
using NCalc.DependencyInjection;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var apiUri = new Uri($"https://{builder.Configuration["Services:api:https:0"]}");

builder.Services.AddServiceDiscovery();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});

builder.Services.Configure<ConfigurationServiceEndpointProviderOptions>(static options =>
{
    options.SectionName = "Services";
    options.ShouldApplyHostNameMetadata = static _ => true;
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    configuration.SnackbarConfiguration.PreventDuplicates = false;
});

builder.Services.AddMudTranslations();

builder.Services.AddMemoryCache();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddNCalc();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<FastOperationService>();
builder.Services.AddScoped<PlaceService>();
builder.Services.AddScoped<RegularOperationService>();
builder.Services.AddScoped<DebtService>();
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<CarEventService>();

builder.Services.AddTransient<RefreshTokenHandler>();
builder.Services.AddHttpClient<AuthenticationService>(client => client.BaseAddress = apiUri);
builder.Services.AddHttpClient<JwtParser>(client => client.BaseAddress = apiUri);

builder.Services.AddHttpClient<MoneyClient>(client => client.BaseAddress = apiUri)
    .AddHttpMessageHandler<RefreshTokenHandler>();

var culture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
