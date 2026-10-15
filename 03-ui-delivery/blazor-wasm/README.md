# Blazor WebAssembly

## What this is

A standalone Blazor WebAssembly client that downloads the .NET runtime and the app into the browser and renders everything there. The browser cannot open a SQL connection, and must never hold a connection string, so the example also carries a minimal API, `Chinook.BlazorWasm.Api`, which the client calls over HTTP for every piece of data.

The API is **not a hosting shell**. The client is a set of static files (`dotnet publish` produces a `wwwroot` you can put on any static host or CDN), and it does not need the API to serve it. The API is just the nearest honest data source: the smallest thing that can stand between a browser and SQL Server.

## When I would pick it

When the app has to run without a live server connection between interactions: offline-capable tools, installable PWAs, or UIs with a lot of local state that would be painful to round-trip. It also fits when I want to host the UI as static files on a CDN and scale the API separately. Once loaded, interactions that do not need data are instant.

## When I would not

When first-load time matters, since the runtime download is several megabytes before anything renders. I would also avoid it when the app is mostly forms over data. In that case I have bought an API contract, CORS, auth tokens and two deployables just to show a table, and Razor Pages with htmx does it with one. Anything in the client is visible to the user, so business rules have to be enforced again in the API.

## What to look at first

Start at `src/Chinook.BlazorWasm.Client/Pages/Artists.razor`, which calls `GET artists` with the `HttpClient` registered in `src/Chinook.BlazorWasm.Client/Program.cs`. The client reads the API's address from `wwwroot/appsettings.json`. Then open `src/Chinook.BlazorWasm.Api/Program.cs`. It holds the `/artists` endpoint and the CORS policy that lets the client's dev origins (`http://localhost:5075`, `https://localhost:7032`, read from `Cors:ClientOrigins`) call it. The response record is declared on both sides on purpose: the client and the API share a JSON contract, not a project.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`, in the API only. The context and entities in `src/Chinook.BlazorWasm.Api/Data/` were scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool (`dotnet tool restore`). The client has no data access of its own.

The API connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.BlazorWasm.Api/appsettings.Development.json`. To run it, start the database with `database/start-db.sh` and `database/init-db.sh`, then `dotnet run --project src/Chinook.BlazorWasm.Api` (port 5073) and `dotnet run --project src/Chinook.BlazorWasm.Client` (port 5075), and open `/artists` on the client.

## Status

Scaffolded. Use cases not yet implemented.
