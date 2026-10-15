# .NET Architecture Examples

A collection of small, complete .NET applications that each show one architectural choice. Every example builds the same domain, so you can open two folders side by side and see what actually changes when you pick one approach over another.

I built this because most architecture catalogs compare things that are not alternatives. "Microservices vs CQRS" is not a decision anyone makes; you can have both, neither, or one without the other. This repo sorts the examples by the question each one answers.

## The axes

Each example sits on exactly one axis. If you are choosing between two examples on different axes, you are probably asking the wrong question.

### 1. Code organization

How is the code inside a single process arranged? Layers, slices, or handlers. This axis says nothing about how many deployables you ship.

### 2. Boundaries and distribution

Where are the seams, and are they in-process or over the network? One deployable or many? Synchronous calls or messages?

### 3. UI delivery

Where does rendering happen and what owns the UI state: the server, the browser, or a native shell?

### 4. Hosting and runtime

What runs the process? Long-lived containers, functions that spin up on demand, or a background worker with no HTTP surface at all.

### 5. Cross-cutting concerns

Problems that show up regardless of the other four answers: tenancy, and getting out of a legacy system without a rewrite.

## The domain

Every example works against the Chinook music store database: artists, albums, tracks, playlists, customers, employees, and invoices. It is the sample database most .NET developers have already seen, which means nobody has to learn a domain before they can read an architecture. It still has the shapes that make architecture decisions matter: a write path with real invariants (invoice totals, price snapshots, no duplicate tracks on a playlist), a read-heavy reporting side, a few events worth publishing, and a clean way to do tenancy (one Chinook copy per tenant).

The database runs as SQL Server in a local Docker container. The canonical script, the scripts that start and load the container, and the connection string every example uses are in `database/`. The full domain description, including the tables, the stored procedures the script ships with, and the use cases every example must cover, is in `docs/domain.md`. If an example skips a use case, its README says so and why.

## Repository layout

```
dotnet-architecture-examples/
  README.md
  docs/
    axes.md                      longer discussion of each axis and when it matters
    domain.md                    the shared domain every example implements
    comparing-examples.md        how to diff two examples and what to look for
  01-code-organization/
    n-tier-razor-pages/
    clean-architecture-razor-pages/
    vertical-slice-minimal-apis/
    cqrs-minimal-apis/
    cqrs-event-sourcing-sqlserver/
  02-boundaries-and-distribution/
    modular-monolith-minimal-apis/
    microservices-minimal-apis/
    event-driven-outbox-wolverine/
    orleans-actors/
    bff-gateway-yarp/
  03-ui-delivery/
    razor-pages-htmx/
    blazor-server/
    blazor-wasm/
    blazor-auto/
    maui-mvvm/
  04-hosting-and-runtime/
    aspire-cloud-native-azure/
    azure-functions-durable/
    worker-service/
  05-cross-cutting/
    multi-tenant-saas/
    strangler-fig/
```

Each example folder has its own solution file, its own README, and no project references outside the folder. You can copy any one of them out of this repo and it will still build. I chose duplication over a shared `Domain` project on purpose; a shared project would force every example to agree on entity shapes, and the whole point is that they do not.

## Status

| Example                        | Axis                        | Status  |
| ------------------------------ | --------------------------- | ------- |
| n-tier-razor-pages             | Code organization           | Planned |
| clean-architecture-razor-pages | Code organization           | Planned |
| vertical-slice-minimal-apis    | Code organization           | Planned |
| cqrs-minimal-apis              | Code organization           | Planned |
| cqrs-event-sourcing-sqlserver  | Code organization           | Planned |
| modular-monolith-minimal-apis  | Boundaries and distribution | Planned |
| microservices-minimal-apis     | Boundaries and distribution | Planned |
| event-driven-outbox-wolverine  | Boundaries and distribution | Planned |
| orleans-actors                 | Boundaries and distribution | Planned |
| bff-gateway-yarp               | Boundaries and distribution | Planned |
| razor-pages-htmx               | UI delivery                 | Planned |
| blazor-server                  | UI delivery                 | Planned |
| blazor-wasm                    | UI delivery                 | Planned |
| blazor-auto                    | UI delivery                 | Planned |
| maui-mvvm                      | UI delivery                 | Planned |
| aspire-cloud-native-azure      | Hosting and runtime         | Planned |
| azure-functions-durable        | Hosting and runtime         | Planned |
| worker-service                 | Hosting and runtime         | Planned |
| multi-tenant-saas              | Cross-cutting               | Planned |
| strangler-fig                  | Cross-cutting               | Planned |

## Conventions

- Target framework is `net10.0` everywhere. I will move the whole repo at once when I move it.
- Each example README has the same four sections: what this is, when I would pick it, when I would not, and what to look at first.
- Examples that need infrastructure (Postgres, RabbitMQ, Redis) use .NET Aspire for local orchestration. There is no docker-compose anywhere in the repo.
- Tests live inside each example folder. The test project covers the use cases in `docs/domain.md` so you can confirm two examples really do the same thing.
- No example references another example. If two folders look similar, that is because the decision they show is small, and that is the lesson.

## A note on bias

I have an opinion, and the repo reflects it. My default for a new system is a modular monolith with Razor Pages and htmx, and I reach for the distributed examples only when a concrete constraint forces it. That said, every example here is built to be the best version of its own approach, not a strawman. If you think one of them is set up to fail, open an issue and tell me what a fair version looks like.

## Contributing

Pull requests are welcome, with two conditions. New examples must implement the domain in `docs/domain.md`, and they must sit on exactly one axis. If you cannot say which axis your example belongs to, it is probably two examples.

## License

MIT. See `LICENSE`.