namespace StockPilot.Bmad.Application.Lots;

public class LotQueryService
{
    private readonly ILotRepository _lotRepository;

    public LotQueryService(ILotRepository lotRepository)
    {
        _lotRepository = lotRepository;
    }

    public async Task<IReadOnlyList<LotDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _lotRepository.GetAllDetailsAsync(cancellationToken);
    }

    public async Task<LotDetailDto> GetByIdAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        if (lotId == Guid.Empty)
            throw new LotNotFoundException(lotId);

        var lot = await _lotRepository.GetDetailByIdAsync(lotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(lotId);

        return lot;
    }

    public async Task<IReadOnlyList<LotDetailDto>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _lotRepository.GetDetailsByClientIdAsync(clientId, cancellationToken);
    }

    public async Task<IReadOnlyList<LotDetailDto>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _lotRepository.GetDetailsByWarehouseIdAsync(warehouseId, cancellationToken);
    }
}
