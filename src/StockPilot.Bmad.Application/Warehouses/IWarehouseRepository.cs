using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Application.Warehouses;

public interface IWarehouseRepository
{
    Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
}
