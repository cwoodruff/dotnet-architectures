using Chinook.NTier.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Chinook.NTier.Business;

public static class BusinessServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Business layer and, beneath it, the Data layer. The Web layer calls only this,
    /// so it never has to name a Data type.
    /// </summary>
    public static IServiceCollection AddNTierBusiness(this IServiceCollection services, string connectionString)
    {
        services.AddNTierData(connectionString);
        services.AddScoped<ArtistService>();
        return services;
    }
}
