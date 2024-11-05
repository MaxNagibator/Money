using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Money.ApiClient;
using Money.WebAssembly.CoreLib;
using MudBlazor.Services;
using MudBlazor.Translations;
using NCalc.DependencyInjection;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
Uri apiUri = new("https+http://api/");

builder.AddServiceDefaults();
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PreventDuplicates = false;
});

builder.Services.AddMudTranslations();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddLocalization();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddNCalc();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PlaceService>();
builder.Services.AddTransient<RefreshTokenHandler>();
builder.Services.AddScoped<AuthTokensStore>();
builder.Services.AddScoped<AuthenticationService>();

builder.Services.AddHttpClient<JwtParser>(client =>
    client.BaseAddress = apiUri);

builder.Services.AddHttpClient<MoneyClient>(client =>
    client.BaseAddress = apiUri)
    .AddHttpMessageHandler<RefreshTokenHandler>();

builder.Services.AddHttpClient<MoneyClient>(client =>
    client.BaseAddress = apiUri)
    .AddHttpMessageHandler<RefreshTokenHandler>();

await builder.Build().RunAsync();