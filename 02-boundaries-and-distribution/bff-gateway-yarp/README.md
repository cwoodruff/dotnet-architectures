# BFF gateway with YARP

## What this is

A backend-for-frontend: one gateway in front of two backend APIs, Catalog and Customers, built on the YARP reverse proxy and composed with .NET Aspire. The gateway does two jobs. It proxies `/api/catalog/*` and `/api/customers/*` to the backends, and it serves composed endpoints like `GET /bff/home` that call both backends and return one payload shaped for one screen. The second job is what makes it a BFF and not just a proxy.

## When I would pick it

When a client, especially a mobile or SPA front end, would otherwise make several calls per screen to several services, and I want one place to shape responses, handle auth tokens, and hide the backend topology. I would also pick it when the backends are owned by different teams and the front-end team needs a layer it controls. One BFF per client type keeps each one small.

## When I would not

When there is one backend, or one client that can call the backends directly without pain, because then the gateway is a hop that adds latency and a deployable without adding anything. I would also be careful when the BFF starts collecting business rules. Once it decides things instead of composing things, it has become another backend with a misleading name.

## What to look at first

Start with the `ReverseProxy` section of `src/Chinook.Bff.Gateway/appsettings.json`, which is the whole proxy: two routes, two clusters, and destinations addressed by service name (`https+http://catalog-api`) that Aspire service discovery resolves. Then read `src/Chinook.Bff.Gateway/Bff/HomeEndpoint.cs`, which is the BFF part. It fans out to both backends in parallel and merges the results. Finish with `src/Chinook.Bff.AppHost/AppHost.cs` to see how the names get wired. The walkthrough is `GET /api/catalog/artists` through the gateway, which lands on `GET /artists` in `src/Chinook.Bff.Catalog.Api/Program.cs`.

## Data access

EF Core (`Microsoft.EntityFrameworkCore.SqlServer`) in both backends, each with its own `ChinookDbContext` scaffolded by `dotnet ef dbcontext scaffold --no-onconfiguring` into `Data/`. The gateway has no database.

Both backends connect to one database, `Chinook`, via `ConnectionStrings:Chinook`. Under Aspire, the AppHost declares it with `AddConnectionString("Chinook")`, reads the value from its own `appsettings.Development.json`, and passes it to each backend. It points at the shared `chinook-mssql` container; Aspire does not start a SQL Server of its own. Each backend also carries the connection string in its own `appsettings.Development.json`, so it runs standalone.

To run everything: `dotnet run --project src/Chinook.Bff.AppHost` and open the dashboard. To run without Aspire, start each project on its own. The gateway's `appsettings.Development.json` has a `Services` section that points `catalog-api` and `customers-api` at the backends' default launch ports, and Aspire's injected settings override it when the AppHost is in charge.

## Status

Scaffolded. Use cases not yet implemented.
