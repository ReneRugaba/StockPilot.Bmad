namespace StockPilot.Bmad.Application.Lots;

public class LotDetailDto
{
    public Guid LotId { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;

    public Guid LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;

    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
}

