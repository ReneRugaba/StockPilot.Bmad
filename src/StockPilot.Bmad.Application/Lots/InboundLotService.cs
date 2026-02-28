using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Application.Lots;

public class InboundLotService
{
    // Guid système utilisé comme opérateur par défaut (MVP - pas d'authentification)
    private static readonly Guid SystemOperator = new Guid("00000000-0000-0000-0000-000000000001");

    private readonly ILotRepository _lotRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILocationRepository _locationRepository;

    public InboundLotService(ILotRepository lotRepository, IMovementRepository movementRepository, IClientRepository clientRepository, ILocationRepository locationRepository)
    {
        _lotRepository = lotRepository;
        _movementRepository = movementRepository;
        _clientRepository = clientRepository;
        _locationRepository = locationRepository;
    }

    public async Task<LotDto> InboundAsync(InboundLotRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ClientId == Guid.Empty)
            throw new InboundLotValidationException("ClientId is required.");

        if (request.LocationId == Guid.Empty)
            throw new InboundLotValidationException("LocationId is required.");

        if (string.IsNullOrWhiteSpace(request.Reference))
            throw new InboundLotValidationException("Reference is required.");

        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            throw new InboundLotValidationException("Client does not exist.");

        var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
            throw new InboundLotValidationException("Location does not exist.");

        if (location.Status != LocationStatus.Available)
            throw new InboundLotValidationException("Location is not available.");

        var now = DateTime.UtcNow;

        // Créer le lot
        var lot = Lot.CreateInbound(request.ClientId, request.LocationId, request.Reference, request.Description, now);
        lot = await _lotRepository.CreateAsync(lot, cancellationToken);

        // Créer le mouvement INBOUND
        var movement = Movement.CreateInbound(lot.LotId, request.LocationId, SystemOperator, now);
        await _movementRepository.CreateAsync(movement, cancellationToken);

        // Mettre à jour le statut de l'emplacement en base
        location.SetStatus(LocationStatus.Occupied, now);
        await _locationRepository.UpdateStatusAsync(location, cancellationToken);

        return ToDto(lot);
    }

    private static LotDto ToDto(Lot lot) => new()
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
