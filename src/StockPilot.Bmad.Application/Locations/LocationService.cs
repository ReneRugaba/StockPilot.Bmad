using StockPilot.Bmad.Domain.Locations;

namespace StockPilot.Bmad.Application.Locations;

public class LocationService
{
    private readonly ILocationRepository _repository;

    public LocationService(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.WarehouseId == Guid.Empty)
        {
            throw new LocationValidationException("WarehouseId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new LocationValidationException("Location code is required.");
        }

        var warehouseExists = await _repository.WarehouseExistsAsync(request.WarehouseId, cancellationToken);
        if (!warehouseExists)
        {
            throw new LocationValidationException("Warehouse does not exist.");
        }

        var now = DateTime.UtcNow;
        var location = Location.Create(request.WarehouseId, request.Code, request.Label, now);

        location = await _repository.CreateAsync(location, cancellationToken);

        return ToDto(location);
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _repository.GetAllAsync(cancellationToken);
        return locations.Select(ToDto).ToList();
    }

    public async Task<LocationDto?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _repository.GetByIdAsync(id, cancellationToken);
        return location is null ? null : ToDto(location);
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var locations = await _repository.GetByWarehouseIdAsync(warehouseId, cancellationToken);
        return locations.Select(ToDto).ToList();
    }

    public async Task<LocationDto> UpdateAsync(Guid locationId, UpdateLocationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new LocationValidationException("Location code is required.");

        var location = await _repository.GetByIdAsync(locationId, cancellationToken);
        if (location is null)
            throw new LocationNotFoundException(locationId);

        location.Update(request.Code, request.Label, DateTime.UtcNow);
        await _repository.UpdateAsync(location, cancellationToken);

        return ToDto(location);
    }

    public async Task DisableAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var location = await _repository.GetByIdAsync(locationId, cancellationToken);
        if (location is null)
            throw new LocationNotFoundException(locationId);

        // Idempotent : déjà MAINTENANCE → succès sans erreur
        if (location.Status == LocationStatus.Maintenance)
            return;

        if (location.Status == LocationStatus.Occupied)
            throw new LocationOccupiedException(locationId);

        location.DisableToMaintenance(DateTime.UtcNow);
        await _repository.UpdateAsync(location, cancellationToken);
    }

    private static LocationDto ToDto(Location location) => new()
    {
        LocationId = location.LocationId,
        WarehouseId = location.WarehouseId,
        Code = location.Code,
        Label = location.Label,
        Status = location.Status.ToString().ToUpperInvariant(),
        CreatedAt = location.CreatedAt,
        UpdatedAt = location.UpdatedAt
    };
}
