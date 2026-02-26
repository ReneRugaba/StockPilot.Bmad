namespace StockPilot.Bmad.Application.Clients;

public class ClientDto
{
    public Guid ClientId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

