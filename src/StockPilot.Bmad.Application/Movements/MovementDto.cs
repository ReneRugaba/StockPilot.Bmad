namespace StockPilot.Bmad.Application.Movements;

public class MovementDto
{
    public Guid MovementId { get; init; }
    public Guid LotId { get; init; }
    public string Type { get; init; } = string.Empty;
    public Guid? FromLocationId { get; init; }
    public Guid? ToLocationId { get; init; }
    public DateTime OccurredAt { get; init; }
    public string? Reason { get; init; }
}

