namespace StockPilot.Bmad.Domain.Locations;

public class Location
{
    public Guid LocationId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public LocationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Location() { }

    private Location(Guid locationId, Guid warehouseId, string code, string? label, LocationStatus status, DateTime createdAt, DateTime updatedAt)
    {
        LocationId = locationId;
        WarehouseId = warehouseId;
        Code = code;
        Label = label;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Location Create(Guid warehouseId, string code, string? label, DateTime utcNow)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException("WarehouseId is required", nameof(warehouseId));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Location code is required", nameof(code));
        }

        return new Location(
            Guid.NewGuid(),
            warehouseId,
            code.Trim(),
            string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
            LocationStatus.Available,
            utcNow,
            utcNow);
    }

    public void SetStatus(LocationStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAt = utcNow;
    }

    public void Update(string code, string? label, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Location code is required", nameof(code));

        Code = code.Trim();
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        UpdatedAt = utcNow;
    }

    public void DisableToMaintenance(DateTime utcNow)
    {
        Status = LocationStatus.Maintenance;
        UpdatedAt = utcNow;
    }
}
