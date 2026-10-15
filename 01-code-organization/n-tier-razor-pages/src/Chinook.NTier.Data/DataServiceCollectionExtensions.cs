using Microsoft.Extensions.DependencyInjection;

namespace Chinook.NTier.Data;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddNTierData(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new ArtistRepository(connectionString));
        return services;
    }
}
