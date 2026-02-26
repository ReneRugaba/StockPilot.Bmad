namespace StockPilot.Bmad.Application.Warehouses;

public class WarehouseDto
{
    public Guid WarehouseId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

