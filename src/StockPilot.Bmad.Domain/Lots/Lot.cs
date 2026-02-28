namespace StockPilot.Bmad.Domain.Lots;

public class Lot
{
    public Guid LotId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? LocationId { get; private set; }
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
            throw new ArgumentException("ClientId is required", nameof(clientId));

        if (locationId == Guid.Empty)
            throw new ArgumentException("LocationId is required", nameof(locationId));

        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference is required", nameof(reference));

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

    public void Retrieve(DateTime utcNow)
    {
        if (Status != LotStatus.Stored)
            throw new InvalidOperationException($"Cannot retrieve a lot with status '{Status}'. Lot must be in 'Stored' status.");

        if (LocationId is null)
            throw new InvalidOperationException("Cannot retrieve a lot that has no location assigned.");

        Status = LotStatus.Retrieved;
        LocationId = null;
        UpdatedAt = utcNow;
    }

    public void Move(Guid newLocationId, DateTime utcNow)
    {
        if (Status != LotStatus.Stored)
            throw new InvalidOperationException($"Cannot move a lot with status '{Status}'. Lot must be in 'Stored' status.");

        if (LocationId is null)
            throw new InvalidOperationException("Cannot move a lot that has no location assigned.");

        if (newLocationId == Guid.Empty)
            throw new ArgumentException("NewLocationId is required.", nameof(newLocationId));

        LocationId = newLocationId;
        UpdatedAt = utcNow;
    }

    public void Dispatch(DateTime utcNow)
    {
        if (Status != LotStatus.Stored)
            throw new InvalidOperationException($"Cannot dispatch a lot with status '{Status}'. Lot must be in 'Stored' status.");

        if (LocationId is null)
            throw new InvalidOperationException("Cannot dispatch a lot that has no location assigned.");

        Status = LotStatus.InTransit;
        LocationId = null;
        UpdatedAt = utcNow;
    }

    public void Receive(Guid destinationLocationId, DateTime utcNow)
    {
        if (Status != LotStatus.InTransit)
            throw new InvalidOperationException($"Cannot receive a lot with status '{Status}'. Lot must be in 'InTransit' status.");

        if (destinationLocationId == Guid.Empty)
            throw new ArgumentException("DestinationLocationId is required.", nameof(destinationLocationId));

        Status = LotStatus.Stored;
        LocationId = destinationLocationId;
        UpdatedAt = utcNow;
    }

    public void SetStatus(LotStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAt = utcNow;
    }

    public void UpdateMetadata(string reference, string? description, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference is required", nameof(reference));

        Reference = reference.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UpdatedAt = utcNow;
    }

    public void Archive(DateTime utcNow)
    {
        Status = LotStatus.Archived;
        UpdatedAt = utcNow;
    }
}
