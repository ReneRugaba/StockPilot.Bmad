namespace StockPilot.Bmad.Domain.Lots;

public class Lot
{
    public Guid LotId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid LocationId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public LotStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Lot() { }

    private Lot(Guid lotId, Guid clientId, Guid locationId, string reference, string? description, LotStatus status, DateTime createdAt, DateTime updatedAt)
    {
        LotId = lotId;
        ClientId = clientId;
        LocationId = locationId;
        Reference = reference;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Lot CreateInbound(Guid clientId, Guid locationId, string reference, string? description, DateTime utcNow)
    {
        if (clientId == Guid.Empty)
        {
            throw new ArgumentException("ClientId is required", nameof(clientId));
        }

        if (locationId == Guid.Empty)
        {
            throw new ArgumentException("LocationId is required", nameof(locationId));
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("Reference is required", nameof(reference));
        }

        return new Lot(
            Guid.NewGuid(),
            clientId,
            locationId,
            reference.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            LotStatus.Stored,
            utcNow,
            utcNow);
    }

    public void SetStatus(LotStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAt = utcNow;
    }
}

