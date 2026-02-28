using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Application.Lots;

public class OutboundLotService
{
    private static readonly Guid SystemOperator = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ILotRepository _lotRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ILocationRepository _locationRepository;

    public OutboundLotService(ILotRepository lotRepository, IMovementRepository movementRepository, ILocationRepository locationRepository)
    {
        _lotRepository = lotRepository;
        _movementRepository = movementRepository;
        _locationRepository = locationRepository;
    }

    public async Task<LotDto> OutboundAsync(OutboundLotRequest request, CancellationToken cancellationToken = default)
    {
        if (request.LotId == Guid.Empty)
            throw new OutboundLotValidationException("LotId is required.");

        // Vérifier que le lot existe
        var lot = await _lotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(request.LotId);

        // Vérifier que le lot est STORED
        if (lot.Status != Domain.Lots.LotStatus.Stored)
            throw new OutboundLotValidationException($"Lot is not in 'Stored' status. Current status: '{lot.Status}'.");

        // Vérifier que le lot a un emplacement
        if (lot.LocationId is null)
            throw new OutboundLotValidationException("Lot has no location assigned.");

        var locationId = lot.LocationId.Value;

        // Vérifier que l'emplacement existe et est OCCUPIED
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        if (location is null)
            throw new OutboundLotValidationException("Location associated to this lot does not exist.");

        if (location.Status != LocationStatus.Occupied)
            throw new OutboundLotValidationException("Location is not in 'Occupied' status.");

        var now = DateTime.UtcNow;

        // Appliquer la règle métier du Domain
        lot.Retrieve(now);
        await _lotRepository.UpdateAsync(lot, cancellationToken);

        // Créer le mouvement OUTBOUND
        var movement = Movement.CreateOutbound(lot.LotId, locationId, SystemOperator, now, request.Notes);
        await _movementRepository.CreateAsync(movement, cancellationToken);

        // Remettre l'emplacement en AVAILABLE
        location.SetStatus(LocationStatus.Available, now);
        await _locationRepository.UpdateStatusAsync(location, cancellationToken);

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

