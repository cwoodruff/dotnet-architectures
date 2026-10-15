var builder = DistributedApplication.CreateBuilder(args);

// In development this is the shared chinook-mssql container started by database/start-db.sh,
// read from ConnectionStrings:Chinook in the AppHost's configuration. The AppHost does not start a
// SQL Server container of its own. In Azure, azd provisions an Azure SQL database for this resource
// instead; see infra/README.md.
var chinook = builder.AddConnectionString("Chinook");

var apiService = builder.AddProject<Projects.Chinook_CloudNative_ApiService>("apiservice")
    .WithReference(chinook)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Chinook_CloudNative_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
