namespace StockPilot.Bmad.Api.Locations;

public class CreateLocationRequestDto
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
}

