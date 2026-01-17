using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Accounts;
using UserService.Application.Accounts.Handlers;
using UserService.Application.Contracts;

namespace UserService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }

    public static IServiceCollection AddServiceHandlers(this IServiceCollection services)
    {
        services.AddScoped<StudentHandler>();

        return services;
    }
}