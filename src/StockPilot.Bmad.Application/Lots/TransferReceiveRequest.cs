namespace StockPilot.Bmad.Application.Lots;

public class TransferReceiveRequest
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

