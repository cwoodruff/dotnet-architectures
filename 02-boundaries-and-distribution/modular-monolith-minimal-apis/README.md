# Modular monolith with minimal APIs

## What this is

One deployable, split into three modules (Catalog, Sales, Playlists), each in its own class library with its own DbContext that maps only the tables it owns. The host project references all three modules and does nothing but call `AddXModule` and `MapXEndpoints` on each. Modules never reference each other, and a test enforces that.

## When I would pick it

This is my default for a new system. When one team owns the whole thing and ships it together, I get hard seams between areas of the domain without paying for network calls, distributed transactions, or a deployment pipeline per service. If a module later needs to become a service, its boundary is already drawn and its tables are already its own.

## When I would not

When different parts genuinely need to deploy, scale, or fail independently, or when separate teams are stepping on each other in one release train. I would also skip the ceremony for a small app with one obvious area of the domain, where three projects and three DbContexts buy nothing over a single vertical-slice API.

## What to look at first

Start at `src/Chinook.ModMono.Host/Program.cs` to see how little the host knows. Then open `src/Chinook.ModMono.Modules.Catalog/CatalogModule.cs` and `Features/GetTopArtists.cs` for the walkthrough, `GET /artists`. Compare the three `Data/*DbContext.cs` files to see which tables each module owns: `InvoiceLine.TrackId` and `PlaylistTrack.TrackId` are plain integers in their modules, because `Track` belongs to Catalog. Finally, read `tests/Chinook.ModMono.Tests/ModuleBoundaryTests.cs`, which is the "modules do not reference each other" rule turned into a failing build.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`, one DbContext per module, each scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring` and a `--table` filter (dotnet-ef is a local tool in `.config/dotnet-tools.json`). Catalog maps Artist, Album, Track, Genre, and MediaType; Sales maps Customer, Invoice, InvoiceLine, and Employee; Playlists maps Playlist and PlaylistTrack.

All three modules connect to one database, `Chinook`, through `ConnectionStrings:Chinook` in `src/Chinook.ModMono.Host/appsettings.Development.json`. Sharing the database is fine inside one deployable as long as each module touches only its own tables. Run with `dotnet run --project src/Chinook.ModMono.Host` and open `/artists`.

## Status

Scaffolded. Use cases not yet implemented.
