using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Application.Lots;

public class MoveInternalLotService
{
    private static readonly Guid SystemOperator = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ILotRepository _lotRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IMovementRepository _movementRepository;

    public MoveInternalLotService(ILotRepository lotRepository, ILocationRepository locationRepository, IMovementRepository movementRepository)
    {
        _lotRepository = lotRepository;
        _locationRepository = locationRepository;
        _movementRepository = movementRepository;
    }

    public async Task<LotDto> MoveAsync(MoveInternalLotRequest request, CancellationToken cancellationToken = default)
    {
        if (request.LotId == Guid.Empty)
            throw new MoveInternalLotValidationException("LotId is required.");

        if (request.DestinationLocationId == Guid.Empty)
            throw new MoveInternalLotValidationException("DestinationLocationId is required.");

        // Vérifier que le lot existe
        var lot = await _lotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(request.LotId);

        // Vérifier que le lot est STORED
        if (lot.Status != Domain.Lots.LotStatus.Stored)
            throw new MoveInternalLotValidationException($"Lot is not in 'Stored' status. Current status: '{lot.Status}'.");

        // Vérifier que le lot a un emplacement source
        if (lot.LocationId is null)
            throw new MoveInternalLotValidationException("Lot has no source location assigned.");

        var sourceLocationId = lot.LocationId.Value;

        // Vérifier que la destination existe
        var destination = await _locationRepository.GetByIdAsync(request.DestinationLocationId, cancellationToken);
        if (destination is null)
            throw new LotNotFoundException(request.DestinationLocationId); // 404

        // Vérifier que la destination est AVAILABLE
        if (destination.Status != LocationStatus.Available)
            throw new MoveInternalLotValidationException("Destination location is not available.");

        // Vérifier que source et destination sont dans le même entrepôt
        var source = await _locationRepository.GetByIdAsync(sourceLocationId, cancellationToken);
        if (source is null)
            throw new MoveInternalLotValidationException("Source location does not exist.");

        if (source.WarehouseId != destination.WarehouseId)
            throw new MoveInternalLotValidationException("Source and destination locations must belong to the same warehouse.");

        var now = DateTime.UtcNow;

        // Déplacer le lot
        lot.Move(request.DestinationLocationId, now);
        await _lotRepository.UpdateAsync(lot, cancellationToken);

        // Créer le mouvement INTERNAL_MOVE
        var movement = Movement.CreateInternalMove(lot.LotId, sourceLocationId, request.DestinationLocationId, SystemOperator, now, request.Notes);
        await _movementRepository.CreateAsync(movement, cancellationToken);

        // Source → AVAILABLE
        source.SetStatus(LocationStatus.Available, now);
        await _locationRepository.UpdateStatusAsync(source, cancellationToken);

        // Destination → OCCUPIED
        destination.SetStatus(LocationStatus.Occupied, now);
        await _locationRepository.UpdateStatusAsync(destination, cancellationToken);

        return ToDto(lot);
    }

    private static LotDto ToDto(Domain.Lots.Lot lot) => new()
    {
        LotId = lot.LotId,
        ClientId = lot.ClientId,
        LocationId = lot.LocationId ?? Guid.Empty,
        Reference = lot.Reference,
        Description = lot.Description,
        Status = lot.Status.ToString().ToUpperInvariant(),
        CreatedAt = lot.CreatedAt,
        UpdatedAt = lot.UpdatedAt
    };
}

