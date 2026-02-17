using Azure.Identity;
using FB_App.Application.Common.Interfaces;
using FB_App.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public const string BlazorClientPolicy_ = "BlazorClientPolicy";

    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        builder.Services.AddMemoryCache();
        builder.Services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });

        builder.Services.AddSignalR();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(BlazorClientPolicy_, policy =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["https://localhost:7140", "http://localhost:5168",
                        "https://localhost:5002", "http://localhost:5003"];

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.DocumentName = "v1";
            configure.Title = "FB_App API";
            configure.Version = "v1";
            configure.Description = "FB_App API Documentation";
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
