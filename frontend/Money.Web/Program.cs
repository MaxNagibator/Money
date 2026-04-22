using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Money.ApiClient;
using Money.Web.Services.Authentication;
using MudBlazor.Services;
using MudBlazor.Translations;
using NCalc.DependencyInjection;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var apiEndpoint = builder.Configuration["Services:api:https:0"]
                  ?? throw new InvalidOperationException("Services:api:https:0 is not configured");

Uri apiUri;
if (Uri.TryCreate(apiEndpoint, UriKind.Absolute, out var parsed)
    && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
{
    apiUri = parsed;
}
else
{
    apiUri = new($"https://{apiEndpoint}");
}

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
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<FastOperationService>();
builder.Services.AddScoped<PlaceService>();
builder.Services.AddScoped<RegularOperationService>();
builder.Services.AddScoped<DebtService>();
builder.Services.AddScoped<DebtOwnerService>();
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
