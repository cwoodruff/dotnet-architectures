# Microservices with minimal APIs

## What this is

Three independently deployable minimal APIs (Catalog, Sales, Playlists), composed for local development by a .NET Aspire AppHost and sharing one ServiceDefaults project for telemetry, health checks, and service discovery. Each service has its own process, its own port, and its own DbContext scaffolded for the tables it is meant to own.

## When I would pick it

When separate teams need to ship separate parts of the system on their own schedules, or when one part has load or availability needs the rest does not share. I would also pick it when the organization already has the platform for it, meaning CI/CD per service, tracing, and on-call that understands distributed failures, because without that platform the split costs more than it saves.

## When I would not

When one team owns everything and deploys it together. Then the network is a cost with no benefit, and a modular monolith gives me the same boundaries without retries, partial failures, or eventual consistency. I would also not start here for a new domain whose boundaries I have not learned yet, because moving a seam between services is far harder than moving it between modules.

## What to look at first

Start at `src/Chinook.Micro.AppHost/AppHost.cs`: it registers the shared container with `AddConnectionString("Chinook")` and hands it to every service, and it does not start a SQL Server of its own. Then open `src/Chinook.Micro.Catalog.Api/Program.cs` for the walkthrough, `GET /artists`. Sales and Playlists only answer `GET /` with their name for now.

**All three services point at the same Chinook database in this scaffold. That is the thing this example will later have to stop doing.** A shared database couples the services at the schema level, so they cannot change or deploy independently, and that is most of what microservices are supposed to buy. Giving each service its own data (separate databases or schemas, plus whatever copying or messaging the use cases need across them) is use-case work, not scaffold work. The per-service DbContexts already map only each service's tables, which is the first step.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. Each service has its own `Data/ChinookDbContext.cs`, scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring` and a `--table` filter (dotnet-ef is a local tool in `.config/dotnet-tools.json`): Catalog maps Artist, Album, Track, Genre, and MediaType; Sales maps Customer, Invoice, InvoiceLine, and Employee; Playlists maps Playlist and PlaylistTrack.

All three connect to one database, `Chinook`, through `ConnectionStrings:Chinook`. Under the AppHost the value comes from `src/Chinook.Micro.AppHost/appsettings.Development.json`; each service also carries it in its own `appsettings.Development.json` so it runs standalone with `dotnet run --project src/Chinook.Micro.Catalog.Api`.

## Status

Scaffolded. Use cases not yet implemented.
