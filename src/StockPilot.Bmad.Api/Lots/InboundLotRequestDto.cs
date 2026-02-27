namespace StockPilot.Bmad.Api.Lots;

public class InboundLotRequestDto
{
    public Guid ClientId { get; set; }
    public Guid LocationId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
}

