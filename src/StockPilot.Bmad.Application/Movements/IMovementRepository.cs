using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Application.Movements;

public interface IMovementRepository
{
    Task<Movement> CreateAsync(Movement movement, CancellationToken cancellationToken = default);
}

