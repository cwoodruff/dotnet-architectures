var builder = DistributedApplication.CreateBuilder(args);

// The shared SQL Server container is started by database/start-db.sh, not by Aspire.
// AddConnectionString reads ConnectionStrings:Chinook from this project's configuration
// and hands it to each service; the AppHost never starts its own SQL Server.
var chinook = builder.AddConnectionString("Chinook");

builder.AddProject<Projects.Chinook_Micro_Catalog_Api>("catalog-api")
    .WithReference(chinook);

builder.AddProject<Projects.Chinook_Micro_Sales_Api>("sales-api")
    .WithReference(chinook);

builder.AddProject<Projects.Chinook_Micro_Playlists_Api>("playlists-api")
    .WithReference(chinook);

builder.Build().Run();
