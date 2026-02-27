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
}

