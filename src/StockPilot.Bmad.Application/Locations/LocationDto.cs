namespace StockPilot.Bmad.Application.Locations;

public class LocationDto
{
    public Guid LocationId { get; init; }
    public Guid WarehouseId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Label { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

