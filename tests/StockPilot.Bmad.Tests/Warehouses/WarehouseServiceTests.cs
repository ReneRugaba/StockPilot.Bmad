using Moq;
using StockPilot.Bmad.Application.Warehouses;
using StockPilot.Bmad.Domain.Warehouses;
using Xunit;

namespace StockPilot.Bmad.Tests.Warehouses;

public class WarehouseServiceTests
{
    [Fact]
    public async Task CreateWarehouse_ValidRequest_ReturnsActiveWarehouseDto()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse w, CancellationToken _) => w);

        var service = new WarehouseService(repoMock.Object);

        var request = new CreateWarehouseRequest
        {
            Name = "Main WH",
            Address = "123 Rue du Test"
        };

        var result = await service.CreateWarehouseAsync(request);

        Assert.Equal("Main WH", result.Name);
        Assert.Equal("123 Rue du Test", result.Address);
        Assert.Equal("ACTIVE", result.Status);
        Assert.NotEqual(Guid.Empty, result.WarehouseId);
    }

    [Fact]
    public async Task CreateWarehouse_WithoutName_ThrowsValidationException()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        var service = new WarehouseService(repoMock.Object);

        var request = new CreateWarehouseRequest
        {
            Name = " ",
            Address = "123 Rue du Test"
        };

        await Assert.ThrowsAsync<WarehouseValidationException>(() => service.CreateWarehouseAsync(request));
    }

    [Fact]
    public async Task CreateWarehouse_WithoutAddress_ThrowsValidationException()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        var service = new WarehouseService(repoMock.Object);

        var request = new CreateWarehouseRequest
        {
            Name = "Main WH",
            Address = " "
        };

        await Assert.ThrowsAsync<WarehouseValidationException>(() => service.CreateWarehouseAsync(request));
    }

    [Fact]
    public async Task GetWarehouseById_NotFound_ReturnsNull()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var service = new WarehouseService(repoMock.Object);

        var result = await service.GetWarehouseByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}

