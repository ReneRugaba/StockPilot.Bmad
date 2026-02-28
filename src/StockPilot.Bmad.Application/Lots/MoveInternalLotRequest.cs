namespace StockPilot.Bmad.Application.Lots;

public class MoveInternalLotRequest
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

