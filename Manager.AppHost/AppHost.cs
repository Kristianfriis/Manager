var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Manager_Api>("api")
    .WithHttpHealthCheck("/health");

var web = builder.AddProject<Projects.Manager_Client>("web")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();