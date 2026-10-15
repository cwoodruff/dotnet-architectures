# CQRS with event sourcing on SQL Server

## What this is

A minimal API where the write side stores events instead of current state. A `Playlist` aggregate is rebuilt by replaying its events through `Apply` methods, checks its invariants against that state, and raises new events, which a hand-rolled `SqlEventStore` appends to an `Events` table in a separate `ChinookEvents` database. The read side is Chinook itself, queried with Dapper. There is no event sourcing library here, because the point is to show the mechanism: one table, an append with an optimistic concurrency check, and a load that replays.

I originally planned this example on Marten and dropped it, because Marten is PostgreSQL-only and every example in this repo runs on the same SQL Server container.

## When I would pick it

When the history of changes is a business requirement and not a debugging aid: audit trails that must be complete, questions like "what did this playlist look like last Tuesday", or several read models that need to be rebuilt from the same facts. I would also pick it when the write-side rules are naturally expressed as "given what happened, can this happen next".

## When I would not

When I only need current state, which is most systems. Event sourcing makes every read depend on a projection, every schema change a question of event versioning, and every bug fix a question of what to do with the events already stored. I would not reach for it to get an audit log either; a history table written in the same transaction is far cheaper.

## What to look at first

Start at `src/Chinook.EventSourcing.Api/Domain/Playlist.cs` to see the aggregate's `Apply` methods and how `AddTrack` enforces "a track appears at most once" without any database constraint to fall back on. Then read `EventStore/SqlEventStore.cs`, about a hundred lines, which is the whole event store. `Endpoints/PlaylistEndpoints.cs` shows `POST /playlists` creating an aggregate and appending its `PlaylistCreated` event, and `ReadModel/ArtistQueries.cs` shows `GET /artists` reading Chinook directly.

Projecting the playlist events back into Chinook's `Playlist` and `PlaylistTrack` tables is use-case work, not part of this scaffold. Until it exists, a playlist created here is visible only in the event store.

If I replaced the hand-rolled store with a library, the candidates that run on SQL Server are Eventuous (`Eventuous.SqlServer`), EventFlow (`EventFlow.MsSql`), and SQL Stream Store (`SqlStreamStore.MsSql`, which is no longer actively maintained). I have not evaluated their APIs for this repo; they are noted as options, not recommendations.

## Data access

A hand-rolled event store over `Microsoft.Data.SqlClient` for writes, and Dapper over `Microsoft.Data.SqlClient` for reads. No EF Core, so this example has no scaffolded DbContext and no `dotnet-ef` tool.

Connects to two databases on the shared container:

- `Chinook` via `ConnectionStrings:Chinook`, the read model.
- `ChinookEvents` via `ConnectionStrings:ChinookEvents`, the event store. On startup the API creates the database if it is missing and applies `database/chinook-events.sql` (embedded into the assembly) if the `Events` table is missing. Chinook itself is never modified.

Both connection strings are in `src/Chinook.EventSourcing.Api/appsettings.Development.json`. Start the container with `database/start-db.sh` and `database/init-db.sh` from the repo root, then `dotnet run --project src/Chinook.EventSourcing.Api` and use `Chinook.EventSourcing.Api.http`.

## Status

Scaffolded. Use cases not yet implemented.
