﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using VideoGames.Application.Repositories;
using VideoGames.Persistence.DbContexts;

namespace VideoGames.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        const string DefaultConnectionName = "DefaultConnection";

        var connectionString = configuration.GetConnectionString(DefaultConnectionName);
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }

    public static async Task InitPersistenceAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        try
        {
            var dbContext = provider.GetRequiredService<AppDbContext>();
            await DbInitializer.InitializeAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            var logger = services.GetService<ILogger>();
            logger?.LogCritical(ex, "An error occurred while initializing the application database.");
        }
    }
}
