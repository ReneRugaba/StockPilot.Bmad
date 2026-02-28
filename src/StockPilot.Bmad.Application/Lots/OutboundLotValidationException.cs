namespace StockPilot.Bmad.Application.Lots;

public class OutboundLotValidationException : Exception
{
    public OutboundLotValidationException(string message) : base(message)
    {
    }
}

