namespace StockPilot.Bmad.Application.Lots;

public class LotUpdateValidationException : Exception
{
    public LotUpdateValidationException(string message) : base(message)
    {
    }
}

