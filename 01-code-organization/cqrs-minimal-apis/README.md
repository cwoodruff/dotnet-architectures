# CQRS with minimal APIs

## What this is

A single minimal API where every request is either a command that changes state or a query that reads it, and the two sides use different data access on purpose. Commands go through EF Core and its change tracker. Queries go through Dapper with hand-written SQL shaped exactly like the response. A small hand-written `Dispatcher` finds the one handler for each command or query in DI. There is no mediator library.

## When I would pick it

When reads and writes have clearly different needs: the writes protect invariants and benefit from a rich model, while the reads are wide, joined, and performance-sensitive. Splitting them lets each side use the tool that fits instead of forcing one model to do both. It also gives every use case a named type, which makes the list of things the system does easy to read.

## When I would not

When the reads and writes look the same, which is most CRUD screens. Then I am maintaining two data access styles and a dispatcher for no gain, and a vertical slice calling the DbContext does the same job with less code. I would also not add a dispatcher just to have one; if it never grows pipeline behavior, calling the handler directly is clearer.

## What to look at first

Compare `src/Chinook.Cqrs.Api/Queries/GetTopArtistsQuery.cs`, which runs SQL through Dapper, with `src/Chinook.Cqrs.Api/Commands/CreatePlaylistCommand.cs`, which inserts through EF Core and returns the new Id. Both are reached through `Dispatching/Dispatcher.cs` from `Endpoints/ArtistEndpoints.cs` (`GET /artists`) and `Endpoints/PlaylistEndpoints.cs` (`POST /playlists`). `Program.cs` registers every handler by hand, so the full list of use cases is visible in one place.

## Data access

Both styles, split by side, and that split is the point of the example. **Commands use EF Core** with `Microsoft.EntityFrameworkCore.SqlServer` and a `ChinookDbContext` scaffolded into `Data/` with `dotnet ef dbcontext scaffold ... --no-onconfiguring` (the tool is a local tool in `.config/dotnet-tools.json`; run `dotnet tool restore` first). **Queries use Dapper** over `Microsoft.Data.SqlClient` and never touch the DbContext.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.Cqrs.Api/appsettings.Development.json`. Both sides read and write the same tables; a separate read store is a later step, not part of this scaffold. Start it with `database/start-db.sh` and `database/init-db.sh` from the repo root, then `dotnet run --project src/Chinook.Cqrs.Api` and use `Chinook.Cqrs.Api.http`.

## Status

Scaffolded. Use cases not yet implemented.
