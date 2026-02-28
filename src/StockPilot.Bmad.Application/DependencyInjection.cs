using Microsoft.Extensions.DependencyInjection;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Application.Warehouses;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;

namespace StockPilot.Bmad.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClientService>();
        services.AddScoped<WarehouseService>();
        services.AddScoped<LocationService>();
        services.AddScoped<InboundLotService>();
        services.AddScoped<OutboundLotService>();
        services.AddScoped<LotQueryService>();
        return services;
    }
}
