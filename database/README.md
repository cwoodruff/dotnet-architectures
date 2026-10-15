# Database

Every example in this repo reads and writes the Chinook music store database, running as SQL Server 2022 in one local Docker container named `chinook-mssql`. The applications themselves are never containerized; this container is the only one in the repo.

## Quick start

```bash
cd database
./start-db.sh                    # pull and run the container, wait until it answers SELECT 1
./init-db.sh --tenants US,EU     # load Chinook, plus Chinook_US and Chinook_EU for multi-tenant-saas
```

PowerShell has the same scripts with the same behavior: `./start-db.ps1`, `./init-db.ps1 -Tenants US,EU`, `./stop-db.ps1`.

When you are done, `./stop-db.sh` stops and removes the container. The data lives inside the container, so the next `start-db.sh` / `init-db.sh` starts from a clean copy.

## Files

| File | What it is |
| --- | --- |
| `chinook_with_sprocs-id-pk.sql` | The canonical script: 11 tables, 1 view, 80 stored procedures, all primary keys named `Id`. Do not edit it. |
| `chinook.docker.sql` | Generated from the canonical script by `generate-docker-sql.sh`. This is what `init-db.sh` loads. |
| `generate-docker-sql.sh` | Regenerates `chinook.docker.sql`. Run it again if the canonical script changes. |
| `start-db.sh` / `start-db.ps1` | Create, start, or confirm the `chinook-mssql` container on port 1433. |
| `init-db.sh` / `init-db.ps1` | Load `chinook.docker.sql` into the running container, plus optional tenant copies. |
| `stop-db.sh` / `stop-db.ps1` | Stop and remove the container. |

Both `.sql` files are UTF-16LE with CRLF line endings, as the canonical script was exported. `.gitattributes` marks them binary so git never normalizes them.

### What the generator changes

`generate-docker-sql.sh` makes two edits and fails loudly if either one cannot find the text it expects:

1. It replaces the `CREATE DATABASE [Chinook]` statement, whose `ON PRIMARY` and `LOG ON` clauses carry Windows paths (`C:\Program Files\Microsoft SQL Server\...`), with a bare `CREATE DATABASE [Chinook]`. Those paths do not exist on the Linux container. The `ALTER DATABASE` block and the compatibility level are left alone.
2. It repairs the `AlbumWithArtistName` view. In the canonical script the view still selects `Album.AlbumId` and joins on `Artist.ArtistId`, columns that were renamed to `Id` in this variant, so on a clean load the view fails with `Invalid column name` and never exists. The generator rewrites the view's `SELECT` to use the `Id` columns and keeps the view's original output column names (`AlbumId`, `Title`, `ArtistId`, `Name`).

## Configuration

| Setting | Default | Override |
| --- | --- | --- |
| SA password | `Chinook!Passw0rd` | `CHINOOK_SA_PASSWORD` |
| Host port | `1433` | `CHINOOK_PORT` |
| Container name | `chinook-mssql` | `CHINOOK_CONTAINER` |

The image is pulled as `linux/amd64`; on Apple Silicon Docker runs it under emulation.

## Connection string

Every example uses this connection string under the config key `ConnectionStrings:Chinook`:

```
Server=localhost,1433;Database=Chinook;User Id=sa;Password=Chinook!Passw0rd;TrustServerCertificate=True
```

Examples that need a second database on the same server use additional keys named for that database. Only the `Database=` part changes:

| Key | Database | Used by | Created by |
| --- | --- | --- | --- |
| `ConnectionStrings:Chinook` | `Chinook` | every example | `init-db.sh` |
| `ConnectionStrings:Chinook_US` | `Chinook_US` | multi-tenant-saas | `init-db.sh --tenants US,EU` |
| `ConnectionStrings:Chinook_EU` | `Chinook_EU` | multi-tenant-saas | `init-db.sh --tenants US,EU` |
| `ConnectionStrings:ChinookEvents` | `ChinookEvents` | cqrs-event-sourcing-sqlserver | the example, on startup |
| `ConnectionStrings:ChinookMessaging` | `ChinookMessaging` | event-driven-outbox-wolverine | the example, on startup |
| `ConnectionStrings:ChinookOrleans` | `ChinookOrleans` | orleans-actors | the example, on startup |

No example adds tables to `Chinook` itself. Anything Chinook does not have (an event store, an outbox, Orleans state) lives in its own database on the same server, so the canonical script stays the single description of the domain data.

Each example's `appsettings.Development.json` carries the connection strings it needs with the password inline. **This is a sample repo and the password is deliberately not a secret.** It exists so that cloning the repo and running two scripts is enough to run any example. Do not copy this pattern into a real system.

## Tenant copies

`init-db.sh --tenants US,EU` loads `chinook.docker.sql` once as `Chinook`, then once per suffix with every `[Chinook]` (and the one `N'Chinook'` literal) replaced by `[Chinook_<suffix>]`. Each tenant gets a full, independent copy of the same data, which is what the database-per-tenant example needs.

By default `init-db.sh` skips any database that already exists. Pass `--force` (`-Force` in PowerShell) to drop and recreate it.

## Building without the database

No example needs the database to build, and no test that runs under `build-all.sh` touches it. You need the container only to run an example.
