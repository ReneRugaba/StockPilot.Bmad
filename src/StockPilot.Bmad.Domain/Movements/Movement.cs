namespace StockPilot.Bmad.Domain.Movements;

public class Movement
{
    public Guid MovementId { get; private set; }
    public Guid LotId { get; private set; }
    public MovementType Type { get; private set; }
    public Guid? FromLocationId { get; private set; }
    public Guid? ToLocationId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid PerformedBy { get; private set; }
    public string? Reason { get; private set; }

    private Movement() { }

    private Movement(Guid movementId, Guid lotId, MovementType type, Guid? fromLocationId, Guid? toLocationId, DateTime occurredAt, Guid performedBy, string? reason)
    {
        MovementId = movementId;
        LotId = lotId;
        Type = type;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
        OccurredAt = occurredAt;
        PerformedBy = performedBy;
        Reason = reason;
    }

    public static Movement CreateInbound(Guid lotId, Guid toLocationId, Guid performedBy, DateTime utcNow, string? reason = null)
    {
        if (lotId == Guid.Empty)
        {
            throw new ArgumentException("LotId is required", nameof(lotId));
        }

        if (toLocationId == Guid.Empty)
        {
            throw new ArgumentException("ToLocationId is required", nameof(toLocationId));
        }

        if (performedBy == Guid.Empty)
        {
            throw new ArgumentException("PerformedBy is required", nameof(performedBy));
        }

        return new Movement(
            Guid.NewGuid(),
            lotId,
            MovementType.Inbound,
            null,
            toLocationId,
            utcNow,
            performedBy,
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim());
    }
}

