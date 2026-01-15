using FluentMigrator.Runner;
using Infrastructure.Persistence.Migrations;
using Infrastructure.Persistence.Options.Postgres;
using Infrastructure.Persistence.Repositories.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UserService.Application.Abstractions.Persistence.Repositories;

namespace Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IAccountPasswordRepository, AccountPasswordRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IFollowerRepository, FollowerRepository>();
        services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();

        return services;
    }

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