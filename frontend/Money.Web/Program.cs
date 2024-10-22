using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Money.ApiClient;
using MudBlazor.Services;
using MudBlazor.Translations;
using NCalc.DependencyInjection;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
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
builder.Services.AddScoped<JwtParser>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddTransient<RefreshTokenHandler>();

builder.Services.AddHttpClient("api")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7124/"))
    .AddHttpMessageHandler<RefreshTokenHandler>();

builder.Services.AddHttpClient("api_base")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7124/"));

builder.Services.AddScoped(provider =>
{
    IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("api");
});

builder.Services.AddScoped(provider =>
{
    HttpClient client = provider.GetRequiredService<HttpClient>();
    MoneyClient moneyClient = new(client, Console.WriteLine);
    return moneyClient;
});

await builder.Build().RunAsync();
