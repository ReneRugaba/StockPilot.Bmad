namespace StockPilot.Bmad.Application.Movements;

public class MovementNotFoundException : Exception
{
    public MovementNotFoundException(Guid movementId)
        : base($"Movement '{movementId}' not found.")
    {
    }
}

