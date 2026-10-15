using Chinook.EventDriven.Notifications.Worker;
using Wolverine;
using Wolverine.SqlServer;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var messaging = builder.Configuration.GetConnectionString("ChinookMessaging")
    ?? throw new InvalidOperationException("Connection string 'ChinookMessaging' is not configured.");

MessagingDatabase.EnsureExists(messaging);

builder.UseWolverine(opts =>
{
    // Same transport schema as Sales.Api, separate envelope storage schema.
    opts.UseSqlServerPersistenceAndTransport(messaging, schema: "notifications", transportSchema: "transport")
        .AutoProvision();

    opts.ListenToSqlServerQueue("notifications");
    opts.Policies.UseDurableInboxOnAllListeners();
});

var host = builder.Build();
host.Run();
