var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder.AddProject<Projects.Web>("web");

builder.AddProject<Projects.FBUI>("wasm")
    .WithReference(webApi)
    .WithEnvironment("ApiBaseAddress", webApi.GetEndpoint("https"));

await builder.Build().RunAsync();
