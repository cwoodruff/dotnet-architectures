# Orleans actors

## What this is

A virtual-actor system built with Microsoft Orleans. Each playlist and each artist is a grain: a single-threaded object with its own state, activated on demand somewhere in a cluster of silos and addressed by id. A separate web API is an Orleans client that calls grains over the network, so the unit of distribution is the entity, not the service.

## When I would pick it

When the state is naturally per entity, hot, and touched by many concurrent callers, like a playlist being edited live by several people. A grain serializes calls for its id, which removes a whole class of concurrency bugs without database locks. I would also pick it when I need to scale out stateful work and want the runtime to place and move activations for me.

## When I would not

When the work is mostly reports and queries across many entities, because grains are bad at "give me everything where...". I would also avoid it for a small team that does not need a cluster. Orleans adds a silo process, a membership table, grain storage, and serialization rules, and that is a lot of machinery to show one list of artists.

## What to look at first

Start with `src/Chinook.Orleans.Grains.Abstractions/IPlaylistGrain.cs`, then its implementation in `src/Chinook.Orleans.Grains/PlaylistGrain.cs`, then `GET /playlists/{id}/ping` in `src/Chinook.Orleans.Api/Program.cs`. That one call goes from the API, through the cluster membership table, to the silo hosting playlist `{id}`, and writes the grain's state to SQL Server before it answers. Note that `GET /artists` in the same file reads Chinook directly with EF Core. Not every read has to go through a grain, and pretending otherwise is a common mistake.

## Data access

EF Core (`Microsoft.EntityFrameworkCore.SqlServer`) in the API for the walkthrough, with a `ChinookDbContext` scaffolded by `dotnet ef dbcontext scaffold --no-onconfiguring` into `src/Chinook.Orleans.Api/Data/`. Orleans clustering and grain storage use the Orleans ADO.NET providers (`Microsoft.Orleans.Clustering.AdoNet`, `Microsoft.Orleans.Persistence.AdoNet`), which resolved and run on .NET 10, so no fallback was needed.

Connects to two databases: `Chinook` (`ConnectionStrings:Chinook`, API only) and `ChinookOrleans` (`ConnectionStrings:ChinookOrleans`, silo and API). `ChinookOrleans` is never created by `database/init-db.sh`. On startup the silo creates the database if it is missing and applies the three official Orleans scripts in `database/` (`SQLServer-Main.sql`, `SQLServer-Clustering.sql`, `SQLServer-Persistence.sql`, taken from the dotnet/orleans repository at tag `v10.4.0`) if the `OrleansQuery` table is not there.

To run it, start the container, then start the silo before the API:

```bash
dotnet run --project src/Chinook.Orleans.Silo
dotnet run --project src/Chinook.Orleans.Api      # in a second terminal
curl http://localhost:5173/playlists/1/ping
```

## Status

Scaffolded. Use cases not yet implemented.
