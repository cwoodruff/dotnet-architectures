namespace Chinook.MultiTenant.Api.Tenancy;

/// <summary>The tenant for the current request. Read-only to everything except the middleware.</summary>
public interface ITenantContext
{
    Tenant? Current { get; }
}

/// <summary>Scoped per request. <see cref="TenantResolutionMiddleware"/> is the only writer.</summary>
public sealed class TenantContext : ITenantContext
{
    public Tenant? Current { get; set; }
}
