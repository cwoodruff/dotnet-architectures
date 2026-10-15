using Chinook.Orleans.Grains;
using Chinook.Orleans.Silo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Content root is the output folder so appsettings*.json and the database/ scripts are found
// no matter which directory the silo is started from.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

var orleansConnection = builder.Configuration.GetConnectionString("ChinookOrleans")
    ?? throw new InvalidOperationException("Connection string 'ChinookOrleans' is not configured.");

builder.UseOrleans(silo =>
{
    silo.Configure<global::Orleans.Configuration.ClusterOptions>(options =>
    {
        options.ClusterId = builder.Configuration["Orleans:ClusterId"] ?? "chinook";
        options.ServiceId = builder.Configuration["Orleans:ServiceId"] ?? "chinook-orleans";
    });

    silo.UseAdoNetClustering(options =>
    {
        options.Invariant = "Microsoft.Data.SqlClient";
        options.ConnectionString = orleansConnection;
    });

    silo.AddAdoNetGrainStorage(GrainStorage.Name, options =>
    {
        options.Invariant = "Microsoft.Data.SqlClient";
        options.ConnectionString = orleansConnection;
    });
});

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Chinook.Orleans.Silo");
await OrleansSchema.EnsureAsync(orleansConnection, logger);

await host.RunAsync();
