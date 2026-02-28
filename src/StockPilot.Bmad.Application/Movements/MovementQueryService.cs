using StockPilot.Bmad.Application.Lots;

namespace StockPilot.Bmad.Application.Movements;

public class MovementQueryService
{
    private readonly IMovementRepository _movementRepository;
    private readonly ILotRepository _lotRepository;

    public MovementQueryService(IMovementRepository movementRepository, ILotRepository lotRepository)
    {
        _movementRepository = movementRepository;
        _lotRepository = lotRepository;
    }

    public async Task<IReadOnlyList<MovementDto>> GetByLotIdAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        if (lotId == Guid.Empty)
            throw new LotNotFoundException(lotId);

        var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(lotId);

        return await _movementRepository.GetByLotIdAsync(lotId, cancellationToken);
    }

    public async Task<MovementDto> GetByIdAsync(Guid movementId, CancellationToken cancellationToken = default)
    {
        var movement = await _movementRepository.GetByIdAsync(movementId, cancellationToken);
        if (movement is null)
            throw new MovementNotFoundException(movementId);

        return movement;
    }
}

