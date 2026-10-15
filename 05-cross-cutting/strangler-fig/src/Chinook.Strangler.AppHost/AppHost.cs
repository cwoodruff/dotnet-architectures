var builder = DistributedApplication.CreateBuilder(args);

// The shared chinook-mssql container started by database/start-db.sh. Both Legacy and Modern read
// the same database on purpose; see the README.
var chinook = builder.AddConnectionString("Chinook");

var legacy = builder.AddProject<Projects.Chinook_Strangler_Legacy>("legacy")
    .WithReference(chinook);

var modern = builder.AddProject<Projects.Chinook_Strangler_Modern_Api>("modern-api")
    .WithReference(chinook);

builder.AddProject<Projects.Chinook_Strangler_Facade>("facade")
    .WithReference(legacy)
    .WithReference(modern)
    .WithExternalHttpEndpoints();

builder.Build().Run();
