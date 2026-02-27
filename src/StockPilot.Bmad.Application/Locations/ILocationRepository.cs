using StockPilot.Bmad.Domain.Locations;

namespace StockPilot.Bmad.Application.Locations;

public interface ILocationRepository
{
    Task<Location> CreateAsync(Location location, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Location>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<bool> WarehouseExistsAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<bool> LocationExistsAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Location location, CancellationToken cancellationToken = default);
}
