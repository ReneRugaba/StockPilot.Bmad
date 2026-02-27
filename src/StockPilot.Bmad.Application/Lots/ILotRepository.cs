using StockPilot.Bmad.Domain.Lots;

namespace StockPilot.Bmad.Application.Lots;

public interface ILotRepository
{
    Task<Lot> CreateAsync(Lot lot, CancellationToken cancellationToken = default);
    Task<LotDetailDto?> GetDetailByIdAsync(Guid lotId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LotDetailDto>> GetAllDetailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LotDetailDto>> GetDetailsByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LotDetailDto>> GetDetailsByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
}
