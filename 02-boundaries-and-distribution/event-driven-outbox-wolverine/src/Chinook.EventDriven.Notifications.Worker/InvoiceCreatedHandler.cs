using Chinook.EventDriven.Contracts;

namespace Chinook.EventDriven.Notifications.Worker;

/// <summary>
/// Wolverine finds this by convention (a public class ending in "Handler" with a Handle method).
/// For now it only logs; sending a receipt is use-case work.
/// </summary>
public class InvoiceCreatedHandler(ILogger<InvoiceCreatedHandler> logger)
{
    public void Handle(InvoiceCreated message)
    {
        logger.LogInformation(
            "Invoice {InvoiceId} created for customer {CustomerId}: {LineCount} line(s), total {Total}",
            message.InvoiceId, message.CustomerId, message.Lines.Count, message.Total);
    }
}
