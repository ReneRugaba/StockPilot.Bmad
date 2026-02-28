using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Application.Movements;

public interface IMovementRepository
{
    Task<Movement> CreateAsync(Movement movement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MovementDto>> GetByLotIdAsync(Guid lotId, CancellationToken cancellationToken = default);
    Task<MovementDto?> GetByIdAsync(Guid movementId, CancellationToken cancellationToken = default);
}
