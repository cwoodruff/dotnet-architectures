using Chinook.Clean.Application.Abstractions;
using Chinook.Clean.Infrastructure.Persistence;
using Chinook.Clean.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chinook.Clean.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IArtistRepository, ArtistRepository>();
        return services;
    }
}
