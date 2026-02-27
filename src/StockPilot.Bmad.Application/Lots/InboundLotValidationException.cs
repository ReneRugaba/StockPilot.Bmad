namespace StockPilot.Bmad.Application.Lots;

public class InboundLotValidationException : Exception
{
    public InboundLotValidationException(string message) : base(message)
    {
    }
}

