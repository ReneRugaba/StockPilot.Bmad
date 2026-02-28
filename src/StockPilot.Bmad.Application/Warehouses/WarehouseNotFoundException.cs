namespace StockPilot.Bmad.Application.Warehouses;

public class WarehouseNotFoundException : Exception
{
    public WarehouseNotFoundException(Guid warehouseId)
        : base($"Warehouse '{warehouseId}' not found.")
    {
    }
}

