# The axes

The examples in this repo are sorted by the question each one answers. I use five questions, and every example sits on exactly one of them. Comparing two examples on the same axis tells you something. Comparing two on different axes usually tells you that you are asking two questions at once.

Each section below covers what the axis asks, what changes between its examples, what stays fixed, and the signals I use to pick one.

## 1. Code organization

**The question:** inside a single process, how is the code arranged?

**What changes:** where a request's code lives and which direction the dependencies point. N-tier puts it in horizontal layers that call downward. Clean architecture inverts that so the domain depends on nothing. Vertical slices group everything for one feature in one place. CQRS splits the read path from the write path, and event sourcing goes further and stores the writes as a sequence of events instead of current state.

**What stays the same:** one deployable, one database, the same use cases, the same HTTP or page surface. None of these examples says anything about how many processes you run. A modular monolith can use slices inside each module, and a microservice can be layered.

**How I choose:**

- If the team is small and the features are mostly independent screens over the data, I start with vertical slices. The cost of a new feature stays flat as the app grows.
- If the domain rules are the valuable part and they need to outlive the framework and the database, clean architecture earns its extra projects.
- If reads and writes have very different shapes or loads, I split them with CQRS. I add event sourcing only when the history of changes is itself a business requirement, such as audit, temporal queries, or rebuilding projections.

## 2. Boundaries and distribution

**The question:** where are the seams, and does crossing one mean a method call or a network hop?

**What changes:** the number of deployables, how they talk (in-process calls, HTTP, messages, actor calls), and who owns which data. The modular monolith draws hard seams but keeps them in one process. Microservices put the seams on the network. The event-driven example makes the seams asynchronous with an outbox. Orleans makes the unit of distribution an actor instead of a service. The BFF puts a composition layer in front of several backends for one client.

**What stays the same:** the code inside each piece can be organized however you like; that is axis 1. The UI technology does not change either. Every example on this axis exposes plain HTTP endpoints so the comparison is about the seams and nothing else.

**How I choose:**

- If one team owns the whole thing and deploys it together, I keep it a modular monolith and spend the effort on keeping the module boundaries honest.
- If separate teams need to deploy on their own schedules, or one part has scaling needs the rest does not, that is when a seam belongs on the network.
- If a step can finish later without the user waiting, such as notifications, projections, or integrations, messaging with an outbox beats a synchronous call. If the state is naturally per entity and hot, like a playlist being edited live, actors fit.

## 3. UI delivery

**The question:** where does rendering happen, and who owns the UI state?

**What changes:** whether HTML is produced on the server per request (Razor Pages with htmx), held on the server over a live connection (Blazor Server), produced in the browser (Blazor WebAssembly), chosen per component at runtime (Blazor Auto), or drawn by a native shell (MAUI). That choice decides how the UI reaches the data. Server-rendered options query the database directly. Browser and native clients cannot hold a connection string, so they call an API.

**What stays the same:** the data and the walkthrough. Every example shows the same top-ten artists list. Where an example needs an API, that API is deliberately minimal so it does not turn into a second architecture lesson.

**How I choose:**

- If the app is mostly forms and lists, and the users are online, I reach for Razor Pages and htmx first. It ships the least code to the browser and has the fewest moving parts.
- If I need rich client interactivity and the users have a reliable connection, Blazor Server gives me that without an API layer. I accept that each user holds server memory and a circuit.
- If the app must work offline, deploy as static files, or run on a device, I move to WebAssembly or MAUI and accept that I now own an API contract.

## 4. Hosting and runtime

**The question:** what runs the process, and what wakes it up?

**What changes:** the host. Aspire with containers gives long-lived processes with service discovery and telemetry wired in. Azure Functions starts code on demand per trigger and, with Durable Functions, keeps workflow state between steps for you. A worker service is a long-lived process with no HTTP surface at all, which is the point.

**What stays the same:** the domain code and the database. In each example the handler that reads artists looks almost the same as everywhere else. What differs is the code around it: startup, configuration, triggers, and how the platform scales it.

**How I choose:**

- If the work is a steady stream of requests and I want local orchestration that matches production, I use Aspire and containers.
- If the work is bursty or event-triggered and idle most of the time, functions are cheaper, and Durable Functions covers multi-step workflows without me writing a state machine.
- If the work is scheduled or queue-driven and nobody calls it over HTTP, a worker service is the honest shape. Bolting a web host onto it only adds attack surface.

## 5. Cross-cutting concerns

**The question:** what problems show up no matter how I answered the other four?

**What changes:** each example here solves one such problem. Multi-tenancy resolves a tenant per request and routes it to that tenant's database. The strangler fig puts a facade in front of a legacy app and moves routes to a new app one at a time.

**What stays the same:** these examples are deliberately plain on the other axes, with a single web API or a small Aspire composition. That keeps the cross-cutting mechanism the only thing you are looking at.

**How I choose:**

- If customers need their data isolated for compliance, or a tenant needs to be restored on its own, I go database per tenant. If not, a tenant column is simpler, and this example shows the harder end on purpose.
- If I am replacing a system that is still earning money, I strangle it rather than rewrite it. The first step is always the facade, not the new code.

## Reading across axes

The axes are independent, and that is the whole idea. A real system makes a choice on all five: say, vertical slices, inside a modular monolith, rendered with Razor Pages and htmx, hosted in containers, with database-per-tenant isolation. This repo holds the other four answers still while it varies one, so you can see the cost of each choice by itself. When you compare two examples, make sure they sit on the same axis first.
