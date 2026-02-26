using StockPilot.Bmad.Domain.Warehouses;
using Xunit;

namespace StockPilot.Bmad.Tests.Warehouses;

public class WarehouseDomainTests
{
    [Fact]
    public void Create_ValidWarehouse_SetsActiveStatus()
    {
        var now = DateTime.UtcNow;

        var warehouse = Warehouse.Create("Main WH", "123 Rue du Test", now);

        Assert.Equal(WarehouseStatus.Active, warehouse.Status);
        Assert.Equal("Main WH", warehouse.Name);
        Assert.Equal("123 Rue du Test", warehouse.Address);
        Assert.Equal(now, warehouse.CreatedAt);
        Assert.Equal(now, warehouse.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutName_Throws()
    {
        var now = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() => Warehouse.Create(" ", "123 Rue du Test", now));
    }

    [Fact]
    public void Create_WithoutAddress_Throws()
    {
        var now = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() => Warehouse.Create("Main WH", " ", now));
    }
}

