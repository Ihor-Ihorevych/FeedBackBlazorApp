using FB_App.Infrastructure.Data;
using FB_App.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();
app.UseExceptionHandler(options => { });
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

app.UseCors("BlazorClientPolicy");

app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapEndpoints();


app.MapHub<AdminNotificationHub>("/hubs/admin-notifications")
   .RequireCors("BlazorClientPolicy");

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

app.Map("/", () => Results.Redirect("/api"));

await app.RunAsync();
