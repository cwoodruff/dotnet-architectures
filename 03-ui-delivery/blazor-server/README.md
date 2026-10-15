# Blazor Server

## What this is

A Blazor Web App where interactive components run on the server. The browser holds a SignalR connection, called a circuit. Clicks go up as events, and the server sends back UI diffs. The C# that renders the page can query the database directly, because it never leaves the server.

## When I would pick it

When I want rich, stateful interactivity, like live filtering, multi-step forms or dashboards, without building and versioning an API between the UI and the data. It suits internal apps with a known number of users on reliable networks. The team writes C# end to end, and nothing about the data model ships to the browser.

## When I would not

When users are on flaky or high-latency connections, since every interaction is a network round trip and a dropped circuit loses UI state. I would also avoid it for public, high-traffic sites: each connected user holds server memory for the lifetime of their circuit, and scaling out needs sticky sessions. If the app must work offline, it is the wrong model entirely.

## What to look at first

Open `src/Chinook.BlazorServer.Web/Components/Pages/Artists.razor`. It declares `@rendermode InteractiveServer` and injects `IDbContextFactory<ChinookDbContext>`, not a `ChinookDbContext`. The registration is in `Program.cs` (`AddDbContextFactory`).

**Why a factory:** a Blazor Server component lives as long as its circuit, which can be hours, not one HTTP request. A scoped `DbContext` would be shared by every event on the page for that whole time. It would collect tracked entities and break with "a second operation was started on this context" when two events overlap. The factory gives each operation its own short-lived context, which is disposed as soon as the query returns.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`, through `IDbContextFactory<ChinookDbContext>`. The context and entities in `src/Chinook.BlazorServer.Web/Data/` were scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool (`dotnet tool restore`).

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.BlazorServer.Web/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh`, then `dotnet run --project src/Chinook.BlazorServer.Web` and open `/artists`.

## Status

Scaffolded. Use cases not yet implemented.
