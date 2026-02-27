using StockPilot.Bmad.Domain.Lots;

namespace StockPilot.Bmad.Application.Lots;

public interface ILotRepository
{
    Task<Lot> CreateAsync(Lot lot, CancellationToken cancellationToken = default);
}

