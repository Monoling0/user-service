using Grpc.Controllers;
using Grpc.Interceptors;
using Infrastructure.Persistence.Extensions;
using Infrastructure.Persistence.Options.Postgres;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserService.Application.Extensions;
using UserService.Application.Models.Accounts;

namespace UserService;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplication app = BuildApp();

        app.MapGrpcService<UserController>();
        app.MapGrpcReflectionService();

        await app.RunAsync();
    }

    private static WebApplication BuildApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        builder.Services
            .AddOptions<PostgresOptions>()
            .Bind(builder.Configuration.GetSection(PostgresOptions.SectionName));
        builder.Services.AddSingleton(sp =>
        {
            IOptionsMonitor<PostgresOptions>
                postgresOptions = sp.GetRequiredService<IOptionsMonitor<PostgresOptions>>();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(postgresOptions.CurrentValue.ConnectionString);
            dataSourceBuilder.MapEnum<Roles>(pgName: "role");

            return dataSourceBuilder.Build();
        });
        builder.Services
            .AddPostgresMigrations()
            .AddPostgresRepositories()
            .AddServices();

        builder.Services.AddSingleton(_ =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        });

        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ErrorInterceptor>();
        });
        builder.Services.AddGrpcReflection();

        return builder.Build();
    }
}