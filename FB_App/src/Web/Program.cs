using FB_App.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enable CORS for Blazor WebAssembly client
app.UseCors("BlazorClientPolicy");

app.UseStaticFiles();

app.MapDefaultEndpoints();
app.MapEndpoints();

// Enable OpenAPI JSON document generation at runtime
// Document available at: /swagger/v1/swagger.json
app.UseOpenApi(settings =>
{
    settings.DocumentName = "v1";
    settings.Path = "/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/swagger/{documentName}/swagger.json";
});

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.Run();

public partial class Program { }
