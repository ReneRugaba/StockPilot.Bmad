namespace StockPilot.Bmad.Domain.Warehouses;

public class Warehouse
{
    public Guid WarehouseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public WarehouseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Warehouse() { }

    private Warehouse(Guid warehouseId, string name, string address, WarehouseStatus status, DateTime createdAt, DateTime updatedAt)
    {
        WarehouseId = warehouseId;
        Name = name;
        Address = address;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Warehouse Create(string name, string address, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Warehouse name is required", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Warehouse address is required", nameof(address));
        }

        return new Warehouse(
            Guid.NewGuid(),
            name.Trim(),
            address.Trim(),
            WarehouseStatus.Active,
            utcNow,
            utcNow);
    }

    public void SetStatus(WarehouseStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAt = utcNow;
    }
}
