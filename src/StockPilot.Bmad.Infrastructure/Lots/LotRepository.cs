using Microsoft.EntityFrameworkCore;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Domain.Lots;

namespace StockPilot.Bmad.Infrastructure.Lots;

public class LotRepository : ILotRepository
{
    private readonly StockPilotDbContext _dbContext;

    public LotRepository(StockPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Lot> CreateAsync(Lot lot, CancellationToken cancellationToken = default)
    {
        _dbContext.Lots.Add(lot);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return lot;
    }

    public async Task<LotDetailDto?> GetDetailByIdAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lots
            .AsNoTracking()
            .Where(l => l.LotId == lotId)
            .Join(_dbContext.Clients, l => l.ClientId, c => c.ClientId,
                (l, c) => new { Lot = l, Client = c })
            .Join(_dbContext.Locations, lc => lc.Lot.LocationId, loc => loc.LocationId,
                (lc, loc) => new { lc.Lot, lc.Client, Location = loc })
            .Join(_dbContext.Warehouses, llc => llc.Location.WarehouseId, w => w.WarehouseId,
                (llc, w) => new LotDetailDto
                {
                    LotId = llc.Lot.LotId,
                    Reference = llc.Lot.Reference,
                    Description = llc.Lot.Description,
                    Status = llc.Lot.Status.ToString().ToUpperInvariant(),
                    CreatedAt = llc.Lot.CreatedAt,
                    ClientId = llc.Client.ClientId,
                    ClientName = llc.Client.Name,
                    LocationId = llc.Location.LocationId,
                    LocationCode = llc.Location.Code,
                    WarehouseId = w.WarehouseId,
                    WarehouseName = w.Name
                })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LotDetailDto>> GetAllDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await BuildLotDetailQuery()
            .OrderBy(l => l.Reference)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LotDetailDto>> GetDetailsByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await BuildLotDetailQuery()
            .Where(l => l.ClientId == clientId)
            .OrderBy(l => l.Reference)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LotDetailDto>> GetDetailsByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await BuildLotDetailQuery()
            .Where(l => l.WarehouseId == warehouseId)
            .OrderBy(l => l.Reference)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<LotDetailDto> BuildLotDetailQuery()
    {
        return _dbContext.Lots
            .AsNoTracking()
            .Join(_dbContext.Clients, l => l.ClientId, c => c.ClientId,
                (l, c) => new { Lot = l, Client = c })
            .Join(_dbContext.Locations, lc => lc.Lot.LocationId, loc => loc.LocationId,
                (lc, loc) => new { lc.Lot, lc.Client, Location = loc })
            .Join(_dbContext.Warehouses, llc => llc.Location.WarehouseId, w => w.WarehouseId,
                (llc, w) => new LotDetailDto
                {
                    LotId = llc.Lot.LotId,
                    Reference = llc.Lot.Reference,
                    Description = llc.Lot.Description,
                    Status = llc.Lot.Status.ToString().ToUpperInvariant(),
                    CreatedAt = llc.Lot.CreatedAt,
                    ClientId = llc.Client.ClientId,
                    ClientName = llc.Client.Name,
                    LocationId = llc.Location.LocationId,
                    LocationCode = llc.Location.Code,
                    WarehouseId = w.WarehouseId,
                    WarehouseName = w.Name
                });
    }
}

