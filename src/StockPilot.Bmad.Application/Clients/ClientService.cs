using StockPilot.Bmad.Domain.Clients;

namespace StockPilot.Bmad.Application.Clients;

public class ClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ClientValidationException("Client name is required.");
        }

        if (!IsValidEmail(request.ContactEmail))
        {
            throw new ClientValidationException("Contact email is invalid.");
        }

        var now = DateTime.UtcNow;
        var client = Client.Create(request.Name, request.ContactEmail, now);

        client = await _repository.AddAsync(client, cancellationToken);

        return ToDto(client);
    }

    public async Task<IReadOnlyList<ClientDto>> GetClientsAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _repository.GetAllAsync(cancellationToken);
        return clients.Select(ToDto).ToList();
    }

    public async Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken);
        return client is null ? null : ToDto(client);
    }

    public async Task<ClientDto> UpdateAsync(Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ClientValidationException("Client name is required.");

        if (!IsValidEmail(request.ContactEmail))
            throw new ClientValidationException("Contact email is invalid.");

        var client = await _repository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            throw new ClientNotFoundException(clientId);

        client.Update(request.Name, request.ContactEmail, DateTime.UtcNow);
        await _repository.UpdateAsync(client, cancellationToken);

        return ToDto(client);
    }

    public async Task DeactivateAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            throw new ClientNotFoundException(clientId);

        // Idempotent : déjà INACTIVE → succès sans erreur
        if (client.Status == ClientStatus.Inactive)
            return;

        client.SetInactive(DateTime.UtcNow);
        await _repository.UpdateAsync(client, cancellationToken);
    }

    private static ClientDto ToDto(Client client) => new()
    {
        ClientId = client.ClientId,
        Name = client.Name,
        ContactEmail = client.ContactEmail,
        Status = client.Status.ToString().ToUpperInvariant(),
        CreatedAt = client.CreatedAt,
        UpdatedAt = client.UpdatedAt
    };

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var trimmed = email.Trim();

        return trimmed.Contains("@") && trimmed.Contains('.') && trimmed.Length >= 3;
    }
}
