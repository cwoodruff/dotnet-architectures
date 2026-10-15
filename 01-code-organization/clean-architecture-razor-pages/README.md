# Clean architecture with Razor Pages

## What this is

Four projects arranged in rings: Domain in the middle with no dependencies at all, Application around it holding use cases and the interfaces they need, Infrastructure implementing those interfaces with EF Core, and a Razor Pages Web project that wires it together. Every reference points inward. The Domain entities are hand-written classes that know nothing about EF Core, and Infrastructure maps them onto the Chinook tables.

## When I would pick it

When the business rules are the valuable part of the system and I expect them to outlive the current framework, database, or UI. I would also pick it when I want to test those rules without a database, because the use cases depend on interfaces I can fake. It suits a domain with real invariants, like Chinook's invoices, more than a set of screens over tables.

## When I would not

When most of the app is reading and writing rows with little logic in between. Then the extra projects, interfaces, and mappings are ceremony, and a new field touches four places. I would also not pick it for a small team that needs to move fast on features; vertical slices give most of the isolation for a fraction of the structure.

## What to look at first

Start at `src/Chinook.Clean.Web/Pages/Artists.cshtml.cs`, which calls the `GetTopArtists` use case in `src/Chinook.Clean.Application/Artists/GetTopArtists.cs`. That use case depends on `IArtistRepository` in `Application/Abstractions`, implemented by `src/Chinook.Clean.Infrastructure/Repositories/ArtistRepository.cs`. Then open `Persistence/Configurations/ArtistConfiguration.cs` to see how the hand-written `Domain.Artist` is mapped without the Domain project referencing EF Core, and `tests/Chinook.Clean.Tests/DependencyRuleTests.cs`, which turns the dependency rule into a failing test if anyone breaks it.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. `ChinookDbContext` in `src/Chinook.Clean.Infrastructure/Persistence` was scaffolded from the live database with `dotnet ef dbcontext scaffold ... --no-onconfiguring` (the tool is a local tool in `.config/dotnet-tools.json`; run `dotnet tool restore` first). After scaffolding I deleted the generated `Artist` and `Album` classes and pointed those `DbSet`s at the Domain entities, which are mapped by `IEntityTypeConfiguration` classes. The other scaffolded entities stay as generated until a use case needs them in the Domain.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.Clean.Web/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh` from the repo root, then `dotnet run --project src/Chinook.Clean.Web` and open `/Artists`.

## Status

Scaffolded. Use cases not yet implemented.
