using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' is not configured.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: ["ready"]);

        return services;
    }
}
