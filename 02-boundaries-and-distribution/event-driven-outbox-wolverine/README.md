# Event-driven with a Wolverine outbox

## What this is

Two processes that talk only through messages. Sales.Api will publish `InvoiceCreated` when an invoice is written, and Notifications.Worker handles it. Wolverine provides the transactional outbox on the publishing side, the durable inbox on the consuming side, and the transport itself, which is a set of tables in SQL Server rather than a broker. The four event records from `docs/domain.md` live in a Contracts project that both sides reference, and an Aspire AppHost runs the two together.

## When I would pick it

When work can finish after the user's request returns, such as receipts, projections, search indexing, or integrations, and I want the publisher to be unaware of who reacts. The outbox is the reason to pick this over "save, then call the other service": the message is persisted with the state change, so a crash between the two cannot drop it. Using SQL Server as the transport means I get all of that without running a broker, which is a good trade at this scale.

## When I would not

When the caller needs the result now. Then a message plus a reply is a slow, fragile RPC, and a direct call is more honest. I would also not reach for messaging to split a system that a single process could handle. Eventual consistency, retries, idempotent handlers, and a dead-letter queue are real costs, and they only pay off when the decoupling is worth something.

## What to look at first

Start at `src/Chinook.EventDriven.Sales.Api/Program.cs`, the `UseWolverine` block: persistence and transport in `ChinookMessaging`, the durable outbox on every sending endpoint, and `InvoiceCreated` routed to the `notifications` queue. Then open `src/Chinook.EventDriven.Notifications.Worker/Program.cs` and `InvoiceCreatedHandler.cs` for the other end. The walkthrough is `GET /artists` on Sales.Api.

The outbox is wired but not exercised until use case 4 lands, because nothing publishes yet. When it does, one design point needs a decision. Wolverine's outbox is atomic with the business write only when its envelope tables share a transaction with the DbContext, which in practice means the same database. Here the envelopes live in `ChinookMessaging` and the invoice lives in `Chinook`, as the repo's rule against adding tables to Chinook requires. Use case 4 will have to either accept that gap (with idempotent handling to cover it) or move the envelope schema next to the business data, and the README will say which.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer` for business data in Sales.Api; `Data/ChinookDbContext.cs` is scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring` (dotnet-ef is a local tool in `.config/dotnet-tools.json`). Wolverine's SQL Server persistence (`WolverineFx.SqlServer`) holds the outbox, inbox, and queue tables.

Two databases on the shared container:

- `Chinook` via `ConnectionStrings:Chinook` (Sales.Api only).
- `ChinookMessaging` via `ConnectionStrings:ChinookMessaging` (both processes). Each process creates the database on startup if it is missing, and Wolverine provisions its tables: envelope storage in a `sales` and a `notifications` schema, and the shared queue tables in a `transport` schema.

There is no broker container. Wolverine's SQL Server transport was available in the release used here, so the RabbitMQ fallback was not needed. `WolverineFx.RuntimeCompilation` is referenced because Wolverine 6 no longer ships its runtime code generator in the core package.

## Status

Scaffolded. Use cases not yet implemented.
