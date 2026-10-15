using Chinook.EventDriven.Contracts;
using Chinook.EventDriven.Sales.Api;
using Chinook.EventDriven.Sales.Api.Data;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var chinook = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
var messaging = builder.Configuration.GetConnectionString("ChinookMessaging")
    ?? throw new InvalidOperationException("Connection string 'ChinookMessaging' is not configured.");

builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(chinook));

MessagingDatabase.EnsureExists(messaging);

builder.UseWolverine(opts =>
{
    // Outbox, inbox, and the SQL Server transport all live in ChinookMessaging, never in Chinook.
    // Each service keeps its envelope storage in its own schema; the queue tables are shared
    // in the "transport" schema, which is what lets the two processes talk without a broker.
    opts.UseSqlServerPersistenceAndTransport(messaging, schema: "sales", transportSchema: "transport")
        .AutoProvision();

    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

    opts.PublishMessage<InvoiceCreated>().ToSqlServerQueue("notifications");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/artists", async (ChinookDbContext db, CancellationToken cancellationToken) =>
    await db.Artists
        .AsNoTracking()
        .OrderBy(a => a.Name)
        .Take(10)
        .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
        .ToListAsync(cancellationToken));

// POST /invoices (use case 4) will insert the invoice through ChinookDbContext and publish
// InvoiceCreated through IMessageBus; the outbox above is wired but not exercised until then.

app.MapDefaultEndpoints();

app.Run();

public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);
