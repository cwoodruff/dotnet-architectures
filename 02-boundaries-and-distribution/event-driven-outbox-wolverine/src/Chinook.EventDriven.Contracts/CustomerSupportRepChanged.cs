namespace Chinook.EventDriven.Contracts;

/// <summary>Raised by use case 6 when a customer's support rep (Customer.SupportRepId) changes.</summary>
public sealed record CustomerSupportRepChanged(int CustomerId, int? PreviousSupportRepId, int? NewSupportRepId);
