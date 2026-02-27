namespace StockPilot.Bmad.Application.Lots;

public class LotNotFoundException : Exception
{
    public LotNotFoundException(Guid lotId)
        : base($"Lot '{lotId}' not found.")
    {
    }
}

