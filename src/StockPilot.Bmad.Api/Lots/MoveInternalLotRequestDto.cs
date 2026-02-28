namespace StockPilot.Bmad.Api.Lots;

public class MoveInternalLotRequestDto
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

