# Comparing examples

The point of this repo is the diff between two examples. Each folder on its own is a fairly ordinary .NET app. What teaches you something is putting two of them next to each other and watching the same request take a different path.

## The method

1. **Pick two examples on the same axis.** `docs/axes.md` explains why. Comparing `vertical-slice-minimal-apis` with `clean-architecture-razor-pages` shows how code organization changes. Comparing `vertical-slice-minimal-apis` with `blazor-server` mixes two decisions together, and you will not be able to tell which one caused a given difference.
2. **Open them side by side.** Two editor windows, or a diff tool pointed at the two `src/` folders. The file names will not line up, and that is expected. You are comparing the shape of the code, not the lines.
3. **Find use case 4, create an invoice, in each.** It is the best use case for comparing because it has the most going on: it validates a customer, looks up track prices, snapshots `UnitPrice` onto each line, computes `Invoice.Total`, writes two tables in one transaction, and publishes `InvoiceCreated`. Every architecture has to put each of those somewhere. Start from the example's README, whose "What to look at first" section names the entry point.
4. **Trace the request from entry point to the tables.** Follow one `POST` from the endpoint, page handler, function trigger, or message handler down to the code that inserts rows into `Invoice` and `InvoiceLine`. At each step, note:
   - Which file you are in, and which project or module.
   - Where the invariants are checked: the total, the price snapshot, the customer's existence.
   - Where the transaction starts and ends.
   - Where `InvoiceCreated` is raised, and whether it is published before, during, or after the commit.
   - What would have to change if the rule changed, for example a discount line.
5. **Count the hops.** Files touched, types touched, and process boundaries crossed. Neither number is good or bad by itself. A higher number buys you something, such as isolation, testability, or independent deployment, and the example's README should say what. If you cannot tell what it buys, that is worth an issue.
6. **Then do the same with use case 9, the sales report.** It is the read-heavy path, and it is where CQRS, event sourcing, and the distributed examples should look most different from the simple ones.

## What to look for

- **Where the rule lives.** In a domain type, in a handler, in a stored procedure, in the database, or spread across several of these.
- **What the boundary costs.** Mapping code, DTOs, serialization, retries, idempotency keys. Each of these is the price of a seam.
- **What the test needs.** A test that needs the database, a running host, or a broker tells you how coupled the code is to its infrastructure.
- **What you would delete first.** If you were simplifying the example, which project or folder would go? Whatever you name is the part the architecture is paying for.

## File map

The files to open first in each example, once use case 4 is implemented. Until then, each example's README points at the walkthrough route, the top-ten artists query, which is the one request path every scaffold already has.

| Example | Files to open first |
| --- | --- |
| n-tier-razor-pages | to be filled when the example is implemented |
| clean-architecture-razor-pages | to be filled when the example is implemented |
| vertical-slice-minimal-apis | to be filled when the example is implemented |
| cqrs-minimal-apis | to be filled when the example is implemented |
| cqrs-event-sourcing-sqlserver | to be filled when the example is implemented |
| modular-monolith-minimal-apis | to be filled when the example is implemented |
| microservices-minimal-apis | to be filled when the example is implemented |
| event-driven-outbox-wolverine | to be filled when the example is implemented |
| orleans-actors | to be filled when the example is implemented |
| bff-gateway-yarp | to be filled when the example is implemented |
| razor-pages-htmx | to be filled when the example is implemented |
| blazor-server | to be filled when the example is implemented |
| blazor-wasm | to be filled when the example is implemented |
| blazor-auto | to be filled when the example is implemented |
| maui-mvvm | to be filled when the example is implemented |
| aspire-cloud-native-azure | to be filled when the example is implemented |
| azure-functions-durable | to be filled when the example is implemented |
| worker-service | to be filled when the example is implemented |
| multi-tenant-saas | to be filled when the example is implemented |
| strangler-fig | to be filled when the example is implemented |
