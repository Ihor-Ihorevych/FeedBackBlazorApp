var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Web>("web");
builder.AddProject<Projects.FBUI>("wasm");

builder.Build().Run();
