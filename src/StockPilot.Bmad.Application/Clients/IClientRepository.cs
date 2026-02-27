using StockPilot.Bmad.Domain.Clients;

namespace StockPilot.Bmad.Application.Clients;

public interface IClientRepository
{
    Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
