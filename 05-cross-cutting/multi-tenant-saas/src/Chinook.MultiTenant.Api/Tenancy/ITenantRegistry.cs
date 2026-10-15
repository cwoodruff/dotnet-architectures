namespace Chinook.MultiTenant.Api.Tenancy;

public interface ITenantRegistry
{
    /// <summary>Looks up a tenant by key, case-insensitively. Returns false for unknown keys.</summary>
    bool TryGetTenant(string tenantId, out Tenant tenant);
}

/// <summary>
/// Maps tenant keys to connection string names and reads the strings from configuration.
/// In a real system this map would live in a catalog database; here it is two lines.
/// </summary>
public sealed class ConfigurationTenantRegistry(IConfiguration configuration) : ITenantRegistry
{
    private static readonly Dictionary<string, string> ConnectionStringNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["us"] = "Chinook_US",
        ["eu"] = "Chinook_EU",
    };

    public bool TryGetTenant(string tenantId, out Tenant tenant)
    {
        tenant = null!;
        if (!ConnectionStringNames.TryGetValue(tenantId, out var name))
        {
            return false;
        }

        var connectionString = configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' for tenant '{tenantId}' is not configured.");
        tenant = new Tenant(tenantId.ToLowerInvariant(), connectionString);
        return true;
    }
}
