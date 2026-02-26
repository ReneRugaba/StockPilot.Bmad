using Microsoft.Extensions.DependencyInjection;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Application.Warehouses;

namespace StockPilot.Bmad.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClientService>();
        services.AddScoped<WarehouseService>();
        return services;
    }
}
