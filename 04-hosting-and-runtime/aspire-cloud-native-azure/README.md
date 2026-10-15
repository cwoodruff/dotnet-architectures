# Cloud-native with .NET Aspire and Azure

## What this is

The `aspire-starter` template, renamed to `Chinook.CloudNative.*` and pointed at Chinook. An Aspire AppHost composes two services: `ApiService`, a minimal API that reads Chinook with EF Core, and `Web`, a Blazor front end that calls the API through service discovery. Both use the shared `ServiceDefaults` project for OpenTelemetry, health checks, resilience, and service discovery. The hosting model is long-lived containers, orchestrated locally by the AppHost and deployed to Azure Container Apps by `azd`.

## When I would pick it

When the system is a handful of steady, always-on services and I want local development to look like production: one `dotnet run` on the AppHost starts everything with telemetry, a dashboard, and service discovery already wired. I would also pick it when the target is Azure and I want the AppHost to be the single description that `azd` turns into infrastructure, so there is no hand-written deployment config.

## When I would not

When the workload is bursty and mostly idle. Always-on containers cost money while they wait, and functions fit that better. I would also not reach for it for a single small web app, because an AppHost and a ServiceDefaults project are overhead when there is only one thing to orchestrate. And if the target is not Azure, the `azd` half of the story goes away, though the AppHost is still useful locally.

## What to look at first

Start at `src/Chinook.CloudNative.AppHost/AppHost.cs`: `AddConnectionString("Chinook")` brings in the shared database container as an external resource, and `WithReference` hands it to the ApiService. Then `src/Chinook.CloudNative.ApiService/Program.cs` for `GET /artists` and the `AddSqlServerDbContext` call that reads the connection string, then `src/Chinook.CloudNative.Web/Components/Pages/Home.razor` and `ArtistsApiClient.cs`, whose base address is the service name `https+http://apiservice`, not a URL. `infra/README.md` explains what changes for Azure.

To run it, start the database (`database/start-db.sh`, `database/init-db.sh`), then `dotnet run --project src/Chinook.CloudNative.AppHost` and open the `webfrontend` link from the dashboard.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`, registered through the Aspire integration `Aspire.Microsoft.EntityFrameworkCore.SqlServer`. `ChinookDbContext` and its entities are scaffolded from the live database into `src/Chinook.CloudNative.ApiService/Data` (`dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool in `.config/dotnet-tools.json`). The Web project has no database access.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook`. In development the AppHost reads it from `src/Chinook.CloudNative.AppHost/appsettings.Development.json` and passes it to the ApiService. The ApiService's own `appsettings.Development.json` carries it too, so the service also runs without the AppHost. In Azure the same resource becomes an Azure SQL database provisioned by `azd`; see `infra/README.md`.

## Status

Scaffolded. Use cases not yet implemented.
