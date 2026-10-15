# Blazor Auto

## What this is

A Blazor Web App whose interactive components use the Auto render mode. On the first visit, a component is prerendered on the server and then becomes interactive over a SignalR circuit, while the WebAssembly runtime downloads in the background. On later visits it runs in the browser on WebAssembly. The same component code runs in both places, so it needs a data source that works in both. The template produces two projects: `Chinook.BlazorAuto.Web` (the server) and `Chinook.BlazorAuto.Web.Client` (components that can run on WebAssembly).

## When I would pick it

When I want WebAssembly's offline-friendly, low-latency interactivity but cannot make users wait for the runtime download on the first visit. Auto starts fast like Blazor Server, then moves the work to the client and frees the server from holding circuits. It suits public-facing apps with returning users and rich pages.

## When I would not

When I do not need both modes. Auto makes me write every component so it can run with or without direct server access, which means an abstraction plus two implementations for every data call. I also have to reason about which mode a component is in when a bug shows up. If the app is fine as Blazor Server, or fine as WebAssembly, either one alone is simpler.

## What to look at first

Open `src/Chinook.BlazorAuto.Web.Client/Pages/Artists.razor`. It declares `@rendermode InteractiveAuto`, prints which renderer it is running on, and depends only on `IArtistsClient`. Then look at the two implementations:

- `src/Chinook.BlazorAuto.Web.Client/Artists/HttpArtistsClient.cs` is registered in the Client's `Program.cs`. On WebAssembly it calls `GET /api/artists` on the Web project.
- `src/Chinook.BlazorAuto.Web/Artists/ServerArtistsClient.cs` is registered in the Web project's `Program.cs`. During prerendering and over the circuit it runs the same query directly through `IDbContextFactory<ChinookDbContext>`, rather than calling its own HTTP endpoint.

Both paths share the query in `src/Chinook.BlazorAuto.Web/Artists/ArtistQueries.cs`, which also maps the `/api/artists` endpoint. The prerendered page queries once on the server, and the interactive component queries again when it starts. Removing that double fetch with persisted component state is use-case work.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`, in the Web project only. The Client project has no database access; on WebAssembly it uses HTTP. The context and entities in `src/Chinook.BlazorAuto.Web/Data/` were scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool (`dotnet tool restore`). The server uses `AddDbContextFactory` because circuit-hosted components outlive a request scope.

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.BlazorAuto.Web/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh`, then `dotnet run --project src/Chinook.BlazorAuto.Web` and open `/artists`.

## Status

Scaffolded. Use cases not yet implemented.
