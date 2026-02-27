using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Application.Warehouses;
using StockPilot.Bmad.Infrastructure.Clients;
using StockPilot.Bmad.Infrastructure.Locations;
using StockPilot.Bmad.Infrastructure.Lots;
using StockPilot.Bmad.Infrastructure.Movements;
using StockPilot.Bmad.Infrastructure.Warehouses;

namespace StockPilot.Bmad.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<StockPilotDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ILotRepository, LotRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();

        return services;
    }
}
