using Microsoft.EntityFrameworkCore;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Domain.Clients;

namespace StockPilot.Bmad.Infrastructure.Clients;

public class ClientRepository : IClientRepository
{
    private readonly StockPilotDbContext _dbContext;

    public ClientRepository(StockPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClientId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clients
            .AsNoTracking()
            .AnyAsync(c => c.ClientId == id, cancellationToken);
    }
}

