# Vertical slices with minimal APIs

## What this is

A single minimal API project where the code is grouped by feature instead of by layer. Each feature is one file under `Features/` holding its request, response, handler, and endpoint mapping. The only shared code is the scaffolded `ChinookDbContext` in `Data/`. There are no controllers, no repositories, and no service layer, because each slice talks to the DbContext directly.

## When I would pick it

When the app is a collection of mostly independent features and I want adding the next one to cost the same as adding the first. Each slice can choose how much structure it needs, so a simple read stays simple while a complex write can grow its own domain types. It is my default for a new HTTP API with a small team.

## When I would not

When several features share a rich set of business rules that must stay consistent, such as one pricing engine used by many endpoints. Slices make it easy to copy logic instead of sharing it, and that drift is hard to spot. I would also think twice on a team that expects a layered structure, because without a shared convention each slice can end up shaped differently.

## What to look at first

Open `src/Chinook.VerticalSlice.Api/Features/Artists/GetTopArtists.cs`. Everything about `GET /artists` is in that one file, including the `MapGetTopArtists(this IEndpointRouteBuilder)` extension that `Program.cs` calls. Then look at `Program.cs` to see how little it knows: one line to register the slice and one line to map it.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. `Data/ChinookDbContext.cs` and the entities in `Data/Entities/` were scaffolded from the live database with `dotnet ef dbcontext scaffold ... --no-onconfiguring` (the tool is a local tool in `.config/dotnet-tools.json`; run `dotnet tool restore` first). Slices query the context directly and project to their own response types.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.VerticalSlice.Api/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh` from the repo root, then `dotnet run --project src/Chinook.VerticalSlice.Api` and call `GET /artists`.

## Status

Scaffolded. Use cases not yet implemented.
