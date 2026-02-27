namespace StockPilot.Bmad.Application.Lots;

public class LotDto
{
    public Guid LotId { get; init; }
    public Guid ClientId { get; init; }
    public Guid LocationId { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

