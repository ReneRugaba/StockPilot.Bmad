namespace StockPilot.Bmad.Api.Locations;

public class UpdateLocationRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
}

