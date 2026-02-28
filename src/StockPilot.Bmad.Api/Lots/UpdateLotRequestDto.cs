namespace StockPilot.Bmad.Api.Lots;

public class UpdateLotRequestDto
{
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
}

