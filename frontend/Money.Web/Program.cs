using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Money.ApiClient;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<JwtParser>();
builder.Services.AddScoped<RefreshTokenService>();
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
    IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
    var moneyClient = new MoneyClient(factory.CreateClient("api"), Console.WriteLine);
    return moneyClient;
});

await builder.Build().RunAsync();
