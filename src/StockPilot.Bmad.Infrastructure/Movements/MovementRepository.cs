using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Infrastructure.Movements;

public class MovementRepository : IMovementRepository
{
    private readonly StockPilotDbContext _dbContext;

    public MovementRepository(StockPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Movement> CreateAsync(Movement movement, CancellationToken cancellationToken = default)
    {
        _dbContext.Movements.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return movement;
    }
}

