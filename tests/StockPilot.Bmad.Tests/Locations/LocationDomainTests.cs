using StockPilot.Bmad.Domain.Locations;
using Xunit;

namespace StockPilot.Bmad.Tests.Locations;

public class LocationDomainTests
{
    [Fact]
    public void Create_ValidLocation_SetsAvailableStatus()
    {
        var now = DateTime.UtcNow;
        var warehouseId = Guid.NewGuid();

        var location = Location.Create(warehouseId, "A-01-01", "Rack A1", now);

        Assert.Equal(LocationStatus.Available, location.Status);
        Assert.Equal(warehouseId, location.WarehouseId);
        Assert.Equal("A-01-01", location.Code);
        Assert.Equal("Rack A1", location.Label);
        Assert.Equal(now, location.CreatedAt);
        Assert.Equal(now, location.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutWarehouseId_Throws()
    {
        var now = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() => Location.Create(Guid.Empty, "A-01-01", "Rack A1", now));
    }

    [Fact]
    public void Create_WithoutCode_Throws()
    {
        var now = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() => Location.Create(Guid.NewGuid(), " ", "Rack A1", now));
    }
}

