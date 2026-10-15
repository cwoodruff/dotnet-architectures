using Chinook.Bff.Customers.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

// Chinook has no created-at column on Customer, so "newest" means the highest Id.
app.MapGet("/customers/newest", async (ChinookDbContext db, int? count, CancellationToken cancellationToken) =>
    await db.Customers
        .OrderByDescending(c => c.Id)
        .Take(Math.Clamp(count ?? 5, 1, 50))
        .Select(c => new CustomerSummary(c.Id, c.FirstName, c.LastName, c.Country, c.SupportRepId))
        .ToListAsync(cancellationToken));

app.Run();

internal sealed record CustomerSummary(int Id, string FirstName, string LastName, string? Country, int? SupportRepId);
