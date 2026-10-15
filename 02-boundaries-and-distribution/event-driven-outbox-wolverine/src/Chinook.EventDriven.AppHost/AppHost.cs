var builder = DistributedApplication.CreateBuilder(args);

// Both databases live on the shared container started by database/start-db.sh.
// There is no broker container: Wolverine's SQL Server transport uses tables in ChinookMessaging.
var chinook = builder.AddConnectionString("Chinook");
var messaging = builder.AddConnectionString("ChinookMessaging");

builder.AddProject<Projects.Chinook_EventDriven_Sales_Api>("sales-api")
    .WithReference(chinook)
    .WithReference(messaging);

builder.AddProject<Projects.Chinook_EventDriven_Notifications_Worker>("notifications-worker")
    .WithReference(messaging);

builder.Build().Run();
