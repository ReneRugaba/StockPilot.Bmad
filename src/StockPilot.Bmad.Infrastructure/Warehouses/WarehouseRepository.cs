using Microsoft.EntityFrameworkCore;
using StockPilot.Bmad.Application.Warehouses;
using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Infrastructure.Warehouses;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly StockPilotDbContext _dbContext;

    public WarehouseRepository(StockPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        _dbContext.Warehouses.Add(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return warehouse;
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarehouseId == id, cancellationToken);
    }
}

