using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<MovementDto>> GetByLotIdAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movements
            .AsNoTracking()
            .Where(m => m.LotId == lotId)
            .OrderByDescending(m => m.OccurredAt)
            .Select(m => new MovementDto
            {
                MovementId = m.MovementId,
                LotId = m.LotId,
                Type = m.Type.ToString().ToUpperInvariant(),
                FromLocationId = m.FromLocationId,
                ToLocationId = m.ToLocationId,
                OccurredAt = m.OccurredAt,
                Reason = m.Reason
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<MovementDto?> GetByIdAsync(Guid movementId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movements
            .AsNoTracking()
            .Where(m => m.MovementId == movementId)
            .Select(m => new MovementDto
            {
                MovementId = m.MovementId,
                LotId = m.LotId,
                Type = m.Type.ToString().ToUpperInvariant(),
                FromLocationId = m.FromLocationId,
                ToLocationId = m.ToLocationId,
                OccurredAt = m.OccurredAt,
                Reason = m.Reason
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
