# Azure Functions with Durable Functions

## What this is

An isolated-worker Azure Functions app on .NET 10. `GetArtists` is an HTTP-triggered function at `GET /api/artists` that reads Chinook with EF Core. `CreateInvoiceOrchestrator` is a Durable Functions orchestration that runs use case 4 as a sequence of activities, `ValidateCustomer` and then `PriceLines`. Both activities are stubs that return canned values for now. The host decides when code runs: nothing is resident until a trigger fires.

## When I would pick it

When the load is bursty or event-driven and idle most of the time, because I pay per execution instead of for an always-on process. I would pick Durable Functions specifically when a workflow has several steps that must survive restarts, like validate, price, charge, and notify. The orchestration checkpoints after each activity, so I get retries and replay without writing a state machine or an outbox myself.

## When I would not

When the traffic is steady and high, because an always-on container is cheaper and has no cold starts. I would also avoid it when the code needs long-lived in-memory state or connections, or when I want local development to be a plain `dotnet run`. Orchestrator code also has to be deterministic, with no `DateTime.Now`, no random numbers, and no I/O outside activities, and that rule surprises people.

## What to look at first

Start at `src/Chinook.Functions/Program.cs`, which is the whole host setup. Then read `Artists/GetArtists.cs` for the walkthrough route and `Invoices/CreateInvoiceOrchestrator.cs` for the orchestration, its two activity stubs, and `StartCreateInvoice` (`POST /api/invoices`), the HTTP starter that schedules an orchestration and returns the Durable status URLs. Notice that the EF Core query in `GetArtists` looks the same as in the container-hosted examples. The hosting model changes the code around the handler, not the handler itself.

To run it locally you need three things besides the .NET SDK: the database container (`database/start-db.sh` and `database/init-db.sh`), [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) v4 (`func`), and [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite), the local storage emulator that Durable Functions keeps its state in (`AzureWebJobsStorage=UseDevelopmentStorage=true`). Then run `func start` in `src/Chinook.Functions`.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`. `ChinookDbContext` and its entities are scaffolded from the live database into `src/Chinook.Functions/Data` (`dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool in `.config/dotnet-tools.json`).

Connects to one database: `Chinook`. Functions does not read `appsettings.Development.json`, so locally the connection string lives under `ConnectionStrings:Chinook` in `src/Chinook.Functions/local.settings.json`. That file is committed on purpose. It holds only the sample password, which is deliberately not a secret (see `database/README.md`), and the Azurite development-storage setting.

## Status

Scaffolded. Use cases not yet implemented.
