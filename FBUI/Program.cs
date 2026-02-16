using Blazored.LocalStorage;
using FBUI;
using FBUI.ApiClient;
using FBUI.Configuration;
using FBUI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API settings
builder.Services.Configure<ApiSettings>(options =>
{
    options.BaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:5001";
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();
builder.Services.AddSingleton<INotificationTextHelper, NotificationTextHelper>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthStateProvider, AuthStateProvider>();
builder.Services.AddScoped(sp => (AuthenticationStateProvider)sp.GetRequiredService<IAuthStateProvider>());
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("FBApi", (sp, client) =>
{
    var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(apiSettings.BaseAddress);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped<IFBApiClient>(sp =>
{
    var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("FBApi");
    return new FBApiClient(apiSettings.BaseAddress, httpClient);
});

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
