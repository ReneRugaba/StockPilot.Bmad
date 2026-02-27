using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;
using Xunit;

namespace StockPilot.Bmad.Tests.Lots;

public class LotDomainTests
{
    [Fact]
    public void CreateInbound_ValidData_ReturnsLotWithStatusStored()
    {
        var clientId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var lot = Lot.CreateInbound(clientId, locationId, "REF-001", "Description", now);

        Assert.NotEqual(Guid.Empty, lot.LotId);
        Assert.Equal(clientId, lot.ClientId);
        Assert.Equal(locationId, lot.LocationId);
        Assert.Equal("REF-001", lot.Reference);
        Assert.Equal("Description", lot.Description);
        Assert.Equal(LotStatus.Stored, lot.Status);
        Assert.Equal(now, lot.CreatedAt);
    }

    [Fact]
    public void CreateInbound_WithoutClientId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Lot.CreateInbound(Guid.Empty, Guid.NewGuid(), "REF-001", null, DateTime.UtcNow));
    }

    [Fact]
    public void CreateInbound_WithoutLocationId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Lot.CreateInbound(Guid.NewGuid(), Guid.Empty, "REF-001", null, DateTime.UtcNow));
    }

    [Fact]
    public void CreateInbound_WithoutReference_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), " ", null, DateTime.UtcNow));
    }

    [Fact]
    public void CreateInbound_NullDescription_DescriptionIsNull()
    {
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-002", null, DateTime.UtcNow);
        Assert.Null(lot.Description);
    }

    [Fact]
    public void Movement_CreateInbound_ValidData_ReturnsMovementWithInboundType()
    {
        var lotId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var movement = Movement.CreateInbound(lotId, locationId, performedBy, now);

        Assert.NotEqual(Guid.Empty, movement.MovementId);
        Assert.Equal(lotId, movement.LotId);
        Assert.Equal(MovementType.Inbound, movement.Type);
        Assert.Null(movement.FromLocationId);
        Assert.Equal(locationId, movement.ToLocationId);
        Assert.Equal(now, movement.OccurredAt);
    }

    [Fact]
    public void Movement_CreateInbound_WithoutLotId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Movement.CreateInbound(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Movement_CreateInbound_WithoutToLocationId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Movement.CreateInbound(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTime.UtcNow));
    }
}

