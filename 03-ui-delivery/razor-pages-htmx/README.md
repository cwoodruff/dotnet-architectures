# Razor Pages with htmx

## What this is

A server-rendered Razor Pages app that uses htmx for partial page updates. Every response is HTML: the first request renders the whole page, and later interactions ask the server for a fragment and swap it into the DOM. There is no JSON API, no client-side state, no npm and no bundler. One script file sits in `wwwroot/lib/htmx/`.

## When I would pick it

This is my default for a new line-of-business web app. When the UI is mostly lists, forms and detail views, server rendering plus htmx gives me the interactivity users expect while shipping almost no JavaScript. It keeps the whole app in one language and one deployable, and the page handler is the only place a request goes.

## When I would not

When the app has to work offline, or the UI holds a lot of state that changes without talking to the server, like a drag-and-drop editor or a canvas. Every interaction is a round trip, so on high-latency connections a client-rendered UI feels faster. I would also think twice if the team already has a large component library in a SPA framework.

## What to look at first

Open `src/Chinook.Htmx.Web/Pages/Artists.cshtml` and find the button with `hx-get="/Artists?handler=List"` and `hx-target="#artist-list"`. That calls `OnGetListAsync` in `Artists.cshtml.cs`, which returns `Pages/Shared/_ArtistList.cshtml` as a partial. The full-page `OnGetAsync` renders the same partial, so both paths share one piece of markup. The htmx script is included once in `Pages/Shared/_Layout.cshtml`.

htmx version: **2.0.11**, downloaded from jsdelivr (`htmx.org@2.0.11/dist/htmx.min.js`) when the example was scaffolded. To upgrade, replace the file; nothing else references a version.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. `ChinookDbContext` and the entities in `src/Chinook.Htmx.Web/Data/` were scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool (`dotnet tool restore`).

Connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.Htmx.Web/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh`, then `dotnet run --project src/Chinook.Htmx.Web` and open `/Artists`.

## Status

Scaffolded. Use cases not yet implemented.
