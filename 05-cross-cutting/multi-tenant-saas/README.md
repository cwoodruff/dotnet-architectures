# Multi-tenant SaaS, database per tenant

## What this is

A minimal API where every request belongs to a tenant, and every tenant has its own complete copy of the Chinook database. `TenantResolutionMiddleware` resolves the tenant before routing runs: first from the `X-Tenant` header, then from the first path segment. It stores the result in a scoped `ITenantContext`, and `ChinookDbContext` is registered with an options factory that picks that tenant's connection string. Nothing below the middleware knows tenancy exists except the one line that chooses the connection string.

## When I would pick it

When tenants need hard isolation: compliance that requires a customer's data to live in its own database or region, per-tenant backup and restore, or one large tenant whose load should not touch the others. It also makes deleting a tenant trivial. I like that the application code stays tenant-unaware, because there is no `TenantId` column to forget in a `WHERE` clause.

## When I would not

When there are thousands of small tenants. Thousands of databases means thousands of migrations, connection pools, and backups, and a shared schema with a tenant column is far cheaper to run. I would also avoid it when tenants need to see across each other, since reporting across tenants in this model means fanning out to every database.

## What to look at first

Start at `src/Chinook.MultiTenant.Api/Tenancy/TenantResolutionMiddleware.cs`, then the `AddDbContext` call in `Program.cs` that reads `ITenantContext`, then `ConfigurationTenantRegistry.cs`, which maps `us` to `ConnectionStrings:Chinook_US` and `eu` to `ConnectionStrings:Chinook_EU`. The tests in `tests/Chinook.MultiTenant.Tests/TenantResolutionMiddlewareTests.cs` cover the header, path, and missing cases with a fake registry and no database.

The walkthrough route is `GET /artists`, and it accepts the tenant in two ways. `GET /artists` with `X-Tenant: us` uses the header. `GET /eu/artists` uses the path: the middleware moves `/eu` into `PathBase`, so the same `/artists` endpoint matches. The header wins when both are present. With neither, the API returns 400 with a problem details body that names the `X-Tenant` header. `/openapi` is the only tenant-free path.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. `ChinookDbContext` and its entities are scaffolded from the live database into `src/Chinook.MultiTenant.Api/Data` (`dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool in `.config/dotnet-tools.json`). Every tenant database has the same schema, so one model serves them all.

Connects to two databases, one per tenant: `Chinook_US` (`ConnectionStrings:Chinook_US`) and `Chinook_EU` (`ConnectionStrings:Chinook_EU`), both in `src/Chinook.MultiTenant.Api/appsettings.Development.json`. **This example depends on the tenant copies existing.** From the repo root, run `database/start-db.sh` and then `database/init-db.sh --tenants US,EU` before `dotnet run --project src/Chinook.MultiTenant.Api`.

## Status

Scaffolded. Use cases not yet implemented.
