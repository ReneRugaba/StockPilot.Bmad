using Microsoft.EntityFrameworkCore;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Domain.Locations;

namespace StockPilot.Bmad.Infrastructure.Locations;

public class LocationRepository : ILocationRepository
{
    private readonly StockPilotDbContext _dbContext;

    public LocationRepository(StockPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Location> CreateAsync(Location location, CancellationToken cancellationToken = default)
    {
        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return location;
    }

    public async Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .AsNoTracking()
            .OrderBy(l => l.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LocationId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Location>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .AsNoTracking()
            .Where(l => l.WarehouseId == warehouseId)
            .OrderBy(l => l.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> WarehouseExistsAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.WarehouseId == warehouseId, cancellationToken);
    }
}

