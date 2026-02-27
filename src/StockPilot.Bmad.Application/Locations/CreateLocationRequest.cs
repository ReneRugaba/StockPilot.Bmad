namespace StockPilot.Bmad.Application.Locations;

public class CreateLocationRequest
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
}

