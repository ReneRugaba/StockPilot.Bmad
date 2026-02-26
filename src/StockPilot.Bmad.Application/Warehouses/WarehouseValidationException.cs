namespace StockPilot.Bmad.Application.Warehouses;

public class WarehouseValidationException : Exception
{
    public WarehouseValidationException(string message) : base(message)
    {
    }
}

