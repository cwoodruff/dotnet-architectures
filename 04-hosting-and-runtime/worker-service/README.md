# Worker service

## What this is

A .NET Generic Host with no HTTP surface at all. `SalesDigestWorker` is a `BackgroundService` that wakes every thirty seconds, counts the invoices created in the last 24 hours, and puts a digest on a `Channel<T>`-backed `DigestQueue`. A second background service, `DigestPublisher`, reads from that queue and delivers each digest, which for now means writing a log line. No port is opened, no endpoint is mapped, and that is the point of the example.

## When I would pick it

When the work is scheduled, queue-driven, or continuous, and nobody needs to call it over HTTP: digests, cleanup jobs, projections, polling an external system. A worker is the honest shape for that work. It has no web host to secure and no health endpoint pretending to be the product, and it scales by running more copies. The producer and consumer split over a channel also gives me back-pressure inside one process without a broker.

## When I would not

When the work is rare and bursty, because a function that only runs when triggered costs less than a process that sits idle. I would also not use a worker when a user is waiting on the result. If someone has to see the outcome of a request, it belongs behind an endpoint. And if the queue has to survive a restart, an in-memory channel is the wrong tool and I want a real message queue.

## What to look at first

Start at `src/Chinook.Worker/Program.cs`. There is no `WebApplication` and no `Map*` call, only hosted services. Then read `SalesDigestWorker.cs` (the producer, built on a `PeriodicTimer` and `IDbContextFactory`), `DigestQueue.cs` (the bounded channel), and `DigestPublisher.cs` (the consumer). The first log line says zero invoices, and that is correct: the Chinook sample invoices are dated 2007 to 2010.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. `ChinookDbContext` and its entities are scaffolded from the live database into `src/Chinook.Worker/Data` (`dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool in `.config/dotnet-tools.json`). The worker uses `IDbContextFactory<ChinookDbContext>` because it is long-lived and needs a fresh context per tick, not one per request.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.Worker/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh`, then run `dotnet run --project src/Chinook.Worker` and watch the log.

## Status

Scaffolded. Use cases not yet implemented.
