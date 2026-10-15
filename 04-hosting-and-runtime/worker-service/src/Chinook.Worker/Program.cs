using Chinook.Worker;
using Chinook.Worker.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");

// The worker outlives any single unit of work, so it asks a factory for a fresh DbContext per tick.
builder.Services.AddDbContextFactory<ChinookDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<DigestQueue>();
builder.Services.AddHostedService<SalesDigestWorker>();
builder.Services.AddHostedService<DigestPublisher>();

var host = builder.Build();
host.Run();
