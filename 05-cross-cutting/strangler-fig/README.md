# Strangler fig

## What this is

A migration in progress. A Legacy ASP.NET Core MVC app, written the way the systems we replace usually are, sits behind a YARP Facade next to a Modern minimal API. The Facade sends `/artists` to Modern and everything else to Legacy. Users only ever see the Facade's address, so routes can move from Legacy to Modern one at a time without anyone noticing a cutover.

## When I would pick it

When I am replacing a system that is still earning money and cannot be frozen for a rewrite. The Facade lets me ship the first modern route in weeks instead of betting on a big-bang release, and every route I move is a small, reversible change. I would also pick it when the legacy code is too tangled to refactor in place but its behavior is still the specification.

## When I would not

When the old system is small enough to rewrite and cut over in one go, because then the Facade and the period of running both systems are pure overhead. I would also not start one without a commitment to finish. A strangler that stalls at three routes leaves you with two systems, a proxy, and the costs of all three.

## What to look at first

Start with the `ReverseProxy` section of `src/Chinook.Strangler.Facade/appsettings.json`. The route table in the Facade is the migration plan. It has two routes today: `/artists/{**remainder}` goes to Modern, and the catch-all, at a lower priority, goes to Legacy. Moving a path from Legacy to Modern is one line, a new route pointing at the `modern` cluster, and moving it back is deleting that line. Then compare `src/Chinook.Strangler.Legacy/Controllers/ArtistsController.cs` (a controller that opens its own connection and calls `sproc_GetArtist`) with `GET /artists` in `src/Chinook.Strangler.Modern.Api/Program.cs`.

YARP matches paths case-insensitively, so the Modern route captures Legacy's `/Artists` page as well. I left that in on purpose because it is exactly what strangling a route means: once the Facade sends `/artists` to Modern, Legacy's page is unreachable through the Facade even though its code still exists. Legacy's `/Albums?artistId=1` still goes to Legacy, so the app is visibly half-migrated. In a real migration Modern would return the same HTML (or the front end would move with it) before the route flipped. Here Modern returns JSON to keep the scaffold small.

Both apps read the same `Chinook` database on purpose. A strangler migration that starts by splitting the database usually never finishes, because the data migration becomes the project and no route ever moves. Move the routes first, let Modern own its tables once nothing in Legacy writes them, and split the data last.

## Data access

Legacy uses raw ADO.NET (`Microsoft.Data.SqlClient`, no Dapper, no EF) inside its controllers, calling `sproc_GetArtist` and `sproc_GetAlbumByArtist` and deserializing the `JSONDATA` column they return. Modern uses EF Core (`Microsoft.EntityFrameworkCore.SqlServer`) with a `ChinookDbContext` scaffolded by `dotnet ef dbcontext scaffold --no-onconfiguring` into `src/Chinook.Strangler.Modern.Api/Data/`. The Facade has no database.

Both apps connect to one database, `Chinook`, via `ConnectionStrings:Chinook`. Under Aspire, `dotnet run --project src/Chinook.Strangler.AppHost` declares it with `AddConnectionString("Chinook")` against the shared `chinook-mssql` container and does not start a SQL Server of its own. Each app also carries the connection string in its own `appsettings.Development.json`. To run without Aspire, start Legacy, Modern, and the Facade on their default launch ports; the Facade's `Services` section in `appsettings.Development.json` points at them. Legacy and Modern do not redirect to HTTPS, because a redirect would send the browser around the Facade.

## Status

Scaffolded. Use cases not yet implemented.
