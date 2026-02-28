namespace StockPilot.Bmad.Application.Lots;

public class TransferLotValidationException : Exception
{
    public TransferLotValidationException(string message) : base(message)
    {
    }
}

