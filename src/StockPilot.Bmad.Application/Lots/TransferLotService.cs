using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Application.Lots;

public class TransferLotService
{
    private static readonly Guid SystemOperator = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ILotRepository _lotRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IMovementRepository _movementRepository;

    public TransferLotService(ILotRepository lotRepository, ILocationRepository locationRepository, IMovementRepository movementRepository)
    {
        _lotRepository = lotRepository;
        _locationRepository = locationRepository;
        _movementRepository = movementRepository;
    }

    public async Task<LotDto> DispatchAsync(TransferDispatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.LotId == Guid.Empty)
            throw new TransferLotValidationException("LotId is required.");

        if (request.DestinationLocationId == Guid.Empty)
            throw new TransferLotValidationException("DestinationLocationId is required.");

        // Lot doit exister
        var lot = await _lotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(request.LotId);

        // Lot doit être STORED
        if (lot.Status != Domain.Lots.LotStatus.Stored)
            throw new TransferLotValidationException($"Lot is not in 'Stored' status. Current status: '{lot.Status}'.");

        // Lot doit avoir un LocationId source
        if (lot.LocationId is null)
            throw new TransferLotValidationException("Lot has no source location assigned.");

        var sourceLocationId = lot.LocationId.Value;

        // Destination doit exister
        var destination = await _locationRepository.GetByIdAsync(request.DestinationLocationId, cancellationToken);
        if (destination is null)
            throw new LotNotFoundException(request.DestinationLocationId);

        // Destination doit être AVAILABLE
        if (destination.Status != LocationStatus.Available)
            throw new TransferLotValidationException("Destination location is not available.");

        // Source doit exister
        var source = await _locationRepository.GetByIdAsync(sourceLocationId, cancellationToken);
        if (source is null)
            throw new TransferLotValidationException("Source location does not exist.");

        // Source et destination doivent être dans des entrepôts DIFFÉRENTS
        if (source.WarehouseId == destination.WarehouseId)
            throw new TransferLotValidationException("Source and destination must belong to different warehouses for a transfer.");

        var now = DateTime.UtcNow;

        // Dispatch du lot
        lot.Dispatch(now);
        await _lotRepository.UpdateAsync(lot, cancellationToken);

        // Créer le mouvement TRANSFER (dispatch)
        var movement = Movement.CreateTransferDispatch(lot.LotId, sourceLocationId, SystemOperator, now, request.Notes);
        await _movementRepository.CreateAsync(movement, cancellationToken);

        // Source → AVAILABLE
        source.SetStatus(LocationStatus.Available, now);
        await _locationRepository.UpdateStatusAsync(source, cancellationToken);

        return ToDto(lot);
    }

    public async Task<LotDto> ReceiveAsync(TransferReceiveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.LotId == Guid.Empty)
            throw new TransferLotValidationException("LotId is required.");

        if (request.DestinationLocationId == Guid.Empty)
            throw new TransferLotValidationException("DestinationLocationId is required.");

        // Lot doit exister
        var lot = await _lotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(request.LotId);

        // Lot doit être IN_TRANSIT
        if (lot.Status != Domain.Lots.LotStatus.InTransit)
            throw new TransferLotValidationException($"Lot is not in 'InTransit' status. Current status: '{lot.Status}'.");

        // Destination doit exister
        var destination = await _locationRepository.GetByIdAsync(request.DestinationLocationId, cancellationToken);
        if (destination is null)
            throw new LotNotFoundException(request.DestinationLocationId);

        // Destination doit être AVAILABLE
        if (destination.Status != LocationStatus.Available)
            throw new TransferLotValidationException("Destination location is not available.");

        var now = DateTime.UtcNow;

        // Réception du lot
        lot.Receive(request.DestinationLocationId, now);
        await _lotRepository.UpdateAsync(lot, cancellationToken);

        // Créer le mouvement TRANSFER (receive)
        var movement = Movement.CreateTransferReceive(lot.LotId, request.DestinationLocationId, SystemOperator, now, request.Notes);
        await _movementRepository.CreateAsync(movement, cancellationToken);

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

