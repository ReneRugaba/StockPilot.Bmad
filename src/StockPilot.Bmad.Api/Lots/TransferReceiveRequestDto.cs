namespace StockPilot.Bmad.Api.Lots;

public class TransferReceiveRequestDto
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

