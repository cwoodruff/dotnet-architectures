# N-tier with Razor Pages

## What this is

A classic three-layer application: a Razor Pages web project on top, a Business layer in the middle, and a Data layer at the bottom. References point downward only, Web to Business to Data, and each layer has its own types, so the page never sees a row from the database. It is the architecture most .NET line-of-business apps from the last twenty years were built with, which is why it is first.

## When I would pick it

When the team already knows it, the app is mostly forms over data, and the rules are simple enough that a service class per area can hold them. I would also pick it when I am handed an existing database with a library of stored procedures, because the Data layer is the obvious home for calling them. The layers are easy to explain to a new developer on day one.

## When I would not

When the business rules are the valuable part. In n-tier they spread across service classes that depend on the Data layer, so testing a rule means dealing with data access. I would also avoid it when features change independently. A new feature touches every layer, and over time the Business layer becomes a set of thin pass-through methods that exist only because the layering says so.

## What to look at first

Start at `src/Chinook.NTier.Web/Pages/Artists.cshtml.cs`, which calls `ArtistService.GetTopTen()` in `src/Chinook.NTier.Business/ArtistService.cs`, which calls `ArtistRepository.GetTopTen()` in `src/Chinook.NTier.Data/ArtistRepository.cs`. Read the comment above the SQL in the repository. The Chinook stored procedures cannot count albums per artist, so the Data layer already holds one inline SQL statement. That is the first place a stored-procedure-only data layer leaks, and it shows up in the very first query.

One more thing: the compiler does not stop the Web project from using Data types, because project references are transitive. Only `BusinessServiceCollectionExtensions.AddNTierBusiness` stands between the page and the repository. Clean architecture makes that rule enforceable; here it is a convention.

## Data access

Dapper over `Microsoft.Data.SqlClient`, calling the Chinook stored procedures by name where they fit and inline SQL where they do not. Every `sproc_Get*` procedure returns a single `JSONDATA` column containing a JSON array (see `docs/domain.md`), so the use cases that call them will deserialize that string rather than map columns.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.NTier.Web/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh` from the repo root, then `dotnet run --project src/Chinook.NTier.Web` and open `/Artists`.

## Status

Scaffolded. Use cases not yet implemented.
