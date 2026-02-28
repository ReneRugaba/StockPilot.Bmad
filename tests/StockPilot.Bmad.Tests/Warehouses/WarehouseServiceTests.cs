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

    [Fact]
    public async Task UpdateWarehouse_ValidRequest_UpdatesWarehouseAndReturnsDto()
    {
        var now = DateTime.UtcNow;
        var existing = Warehouse.Create("Old", "Old address", now);

        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        repoMock
            .Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new WarehouseService(repoMock.Object);

        var request = new UpdateWarehouseRequest { Name = "New", Address = "New address" };

        var result = await service.UpdateAsync(existing.WarehouseId, request);

        Assert.Equal(existing.WarehouseId, result.WarehouseId);
        Assert.Equal("New", result.Name);
        Assert.Equal("New address", result.Address);

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWarehouse_EmptyName_ThrowsValidationException()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        var service = new WarehouseService(repoMock.Object);

        var request = new UpdateWarehouseRequest { Name = " ", Address = "Addr" };
        await Assert.ThrowsAsync<WarehouseValidationException>(() => service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateWarehouse_NotFound_ThrowsNotFoundException()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var service = new WarehouseService(repoMock.Object);

        var request = new UpdateWarehouseRequest { Name = "X", Address = "Y" };
        await Assert.ThrowsAsync<WarehouseNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task CloseWarehouse_WhenActive_ClosesAndPersists()
    {
        var now = DateTime.UtcNow;
        var existing = Warehouse.Create("Main", "Addr", now);

        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        repoMock
            .Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new WarehouseService(repoMock.Object);

        await service.CloseAsync(existing.WarehouseId);

        Assert.Equal(WarehouseStatus.Closed, existing.Status);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseWarehouse_WhenAlreadyClosed_IsIdempotent_DoesNotPersist()
    {
        var now = DateTime.UtcNow;
        var existing = Warehouse.Create("Main", "Addr", now);
        existing.Close(now);

        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = new WarehouseService(repoMock.Object);

        await service.CloseAsync(existing.WarehouseId);

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CloseWarehouse_NotFound_ThrowsNotFoundException()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var service = new WarehouseService(repoMock.Object);

        await Assert.ThrowsAsync<WarehouseNotFoundException>(() => service.CloseAsync(Guid.NewGuid()));
    }
}
