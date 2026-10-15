using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Chinook.Functions.Invoices;

public sealed record CreateInvoiceRequest(int CustomerId, IReadOnlyList<InvoiceLineRequest> Lines);

public sealed record InvoiceLineRequest(int TrackId, int Quantity);

public sealed record PricedLine(int TrackId, decimal UnitPrice, int Quantity);

public sealed record CreateInvoiceResult(int CustomerId, IReadOnlyList<PricedLine> Lines, decimal Total);

/// <summary>
/// The shape of use case 4 as a durable workflow. Each step is an activity, so the runtime
/// checkpoints after it and can replay the orchestration after a crash. The activities are stubs
/// that return canned values; writing Invoice and InvoiceLine rows is use-case work.
/// </summary>
public static class CreateInvoiceOrchestrator
{
    [Function(nameof(CreateInvoiceOrchestrator))]
    public static async Task<CreateInvoiceResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var request = context.GetInput<CreateInvoiceRequest>()
            ?? throw new InvalidOperationException("CreateInvoiceOrchestrator needs a CreateInvoiceRequest input.");

        var customerIsValid = await context.CallActivityAsync<bool>(nameof(ValidateCustomer), request.CustomerId);
        if (!customerIsValid)
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} does not exist.");
        }

        var lines = await context.CallActivityAsync<List<PricedLine>>(nameof(PriceLines), request.Lines);
        var total = lines.Sum(l => l.UnitPrice * l.Quantity);

        return new CreateInvoiceResult(request.CustomerId, lines, total);
    }

    /// <summary>Stub: will check the Customer table. Always says yes for now.</summary>
    [Function(nameof(ValidateCustomer))]
    public static bool ValidateCustomer([ActivityTrigger] int customerId, FunctionContext executionContext)
    {
        executionContext.GetLogger(nameof(ValidateCustomer))
            .LogInformation("Validating customer {CustomerId} (stub: always valid).", customerId);
        return true;
    }

    /// <summary>Stub: will snapshot Track.UnitPrice onto each line. Uses a canned 0.99 for now.</summary>
    [Function(nameof(PriceLines))]
    public static List<PricedLine> PriceLines([ActivityTrigger] IReadOnlyList<InvoiceLineRequest> lines, FunctionContext executionContext)
    {
        executionContext.GetLogger(nameof(PriceLines))
            .LogInformation("Pricing {LineCount} lines (stub: canned price 0.99).", lines.Count);
        return lines.Select(l => new PricedLine(l.TrackId, 0.99m, l.Quantity)).ToList();
    }

    /// <summary>POST /api/invoices starts an orchestration and returns the Durable status URLs.</summary>
    [Function("StartCreateInvoice")]
    public static async Task<HttpResponseData> Start(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoices")] HttpRequestData request,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        var input = await request.ReadFromJsonAsync<CreateInvoiceRequest>(cancellationToken);
        if (input is null)
        {
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(CreateInvoiceOrchestrator), input, cancellationToken);
        return await client.CreateCheckStatusResponseAsync(request, instanceId, cancellationToken);
    }
}
