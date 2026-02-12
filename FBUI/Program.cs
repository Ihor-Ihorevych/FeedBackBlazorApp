using Blazored.LocalStorage;
using FBUI;
using FBUI.ApiClient;
using FBUI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:5001";

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<TokenStorageService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddScoped<AdminNotificationService>();
builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("FBApi", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped<IFBApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("FBApi");
    return new FBApiClient(apiBaseAddress, httpClient);
});

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
