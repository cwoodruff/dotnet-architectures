namespace Chinook.MultiTenant.Api.Tenancy;

/// <summary>A resolved tenant: its key and the connection string for its own Chinook database.</summary>
public sealed record Tenant(string Id, string ConnectionString);
