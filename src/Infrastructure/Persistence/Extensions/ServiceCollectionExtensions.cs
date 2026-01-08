using FluentMigrator.Runner;
using Infrastructure.Persistence.Migrations;
using Infrastructure.Persistence.Options.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresMigrations(
        this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(sp =>
                {
                    PostgresOptions options = sp
                        .GetRequiredService<IOptions<PostgresOptions>>()
                        .Value;

                    return options.ConnectionString;
                })
                .WithMigrationsIn(typeof(InitialMigration).Assembly));

        services.AddHostedService<MigrationRunnerService>();

        return services;
    }
}