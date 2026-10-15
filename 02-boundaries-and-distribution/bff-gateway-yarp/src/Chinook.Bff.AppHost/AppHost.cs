var builder = DistributedApplication.CreateBuilder(args);

// The shared chinook-mssql container started by database/start-db.sh. Aspire does not start a
// SQL Server of its own; it reads ConnectionStrings:Chinook and hands it to each backend.
var chinook = builder.AddConnectionString("Chinook");

var catalog = builder.AddProject<Projects.Chinook_Bff_Catalog_Api>("catalog-api")
    .WithReference(chinook);

var customers = builder.AddProject<Projects.Chinook_Bff_Customers_Api>("customers-api")
    .WithReference(chinook);

builder.AddProject<Projects.Chinook_Bff_Gateway>("gateway")
    .WithReference(catalog)
    .WithReference(customers)
    .WithExternalHttpEndpoints();

builder.Build().Run();
