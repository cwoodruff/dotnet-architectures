namespace Chinook.EventDriven.Contracts;

/// <summary>Raised by use case 4 once an invoice and its lines are committed.</summary>
public sealed record InvoiceCreated(
    int InvoiceId,
    int CustomerId,
    DateTime InvoiceDate,
    decimal Total,
    string? BillingCountry,
    IReadOnlyList<InvoiceCreatedLine> Lines);

/// <summary>One line of an invoice, with the unit price as it was snapshotted at the time of sale.</summary>
public sealed record InvoiceCreatedLine(int TrackId, decimal UnitPrice, int Quantity);
