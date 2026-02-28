using Moq;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Domain.Locations;
using Xunit;

namespace StockPilot.Bmad.Tests.Locations;

public class LocationServiceTests
{
    [Fact]
    public async Task CreateLocation_ValidRequest_ReturnsAvailableLocationDto()
    {
        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.WarehouseExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location l, CancellationToken _) => l);

        var service = new LocationService(repoMock.Object);

        var request = new CreateLocationRequest
        {
            WarehouseId = Guid.NewGuid(),
            Code = "A-01-01",
            Label = "Rack A1"
        };

        var result = await service.CreateLocationAsync(request);

        Assert.Equal("A-01-01", result.Code);
        Assert.Equal("AVAILABLE", result.Status);
        Assert.NotEqual(Guid.Empty, result.LocationId);
    }

    [Fact]
    public async Task CreateLocation_WithoutWarehouseId_ThrowsValidationException()
    {
        var repoMock = new Mock<ILocationRepository>();
        var service = new LocationService(repoMock.Object);

        var request = new CreateLocationRequest
        {
            WarehouseId = Guid.Empty,
            Code = "A-01-01"
        };

        await Assert.ThrowsAsync<LocationValidationException>(() => service.CreateLocationAsync(request));
    }

    [Fact]
    public async Task CreateLocation_WithoutCode_ThrowsValidationException()
    {
        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.WarehouseExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new LocationService(repoMock.Object);

        var request = new CreateLocationRequest
        {
            WarehouseId = Guid.NewGuid(),
            Code = " "
        };

        await Assert.ThrowsAsync<LocationValidationException>(() => service.CreateLocationAsync(request));
    }

    [Fact]
    public async Task CreateLocation_WithUnknownWarehouse_ThrowsValidationException()
    {
        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.WarehouseExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new LocationService(repoMock.Object);

        var request = new CreateLocationRequest
        {
            WarehouseId = Guid.NewGuid(),
            Code = "A-01-01"
        };

        await Assert.ThrowsAsync<LocationValidationException>(() => service.CreateLocationAsync(request));
    }

    [Fact]
    public async Task GetLocationById_NotFound_ReturnsNull()
    {
        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = new LocationService(repoMock.Object);

        var result = await service.GetLocationByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateLocation_ValidRequest_UpdatesLocationAndReturnsDto()
    {
        var now = DateTime.UtcNow;
        var warehouseId = Guid.NewGuid();
        var existing = Location.Create(warehouseId, "A-01-01", "Rack", now);

        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        repoMock
            .Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new LocationService(repoMock.Object);

        var request = new UpdateLocationRequest { Code = "B-02-02", Label = "Rack B2" };
        var result = await service.UpdateAsync(existing.LocationId, request);

        Assert.Equal(existing.LocationId, result.LocationId);
        Assert.Equal("B-02-02", result.Code);
        Assert.Equal("Rack B2", result.Label);

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLocation_EmptyCode_ThrowsValidationException()
    {
        var repoMock = new Mock<ILocationRepository>();
        var service = new LocationService(repoMock.Object);

        var request = new UpdateLocationRequest { Code = " ", Label = "X" };
        await Assert.ThrowsAsync<LocationValidationException>(() => service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateLocation_NotFound_ThrowsNotFoundException()
    {
        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = new LocationService(repoMock.Object);

        var request = new UpdateLocationRequest { Code = "A-01-01" };
        await Assert.ThrowsAsync<LocationNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task DisableLocation_WhenAvailable_SetsMaintenanceAndPersists()
    {
        var now = DateTime.UtcNow;
        var existing = Location.Create(Guid.NewGuid(), "A-01-01", null, now);

        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        repoMock
            .Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new LocationService(repoMock.Object);

        await service.DisableAsync(existing.LocationId);

        Assert.Equal(LocationStatus.Maintenance, existing.Status);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisableLocation_WhenAlreadyMaintenance_IsIdempotent_DoesNotPersist()
    {
        var now = DateTime.UtcNow;
        var existing = Location.Create(Guid.NewGuid(), "A-01-01", null, now);
        existing.SetStatus(LocationStatus.Maintenance, now);

        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = new LocationService(repoMock.Object);

        await service.DisableAsync(existing.LocationId);

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisableLocation_WhenOccupied_ThrowsOccupiedException()
    {
        var now = DateTime.UtcNow;
        var existing = Location.Create(Guid.NewGuid(), "A-01-01", null, now);
        existing.SetStatus(LocationStatus.Occupied, now);

        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(existing.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = new LocationService(repoMock.Object);

        await Assert.ThrowsAsync<LocationOccupiedException>(() => service.DisableAsync(existing.LocationId));
    }

    [Fact]
    public async Task DisableLocation_NotFound_ThrowsNotFoundException()
    {
        var repoMock = new Mock<ILocationRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = new LocationService(repoMock.Object);

        await Assert.ThrowsAsync<LocationNotFoundException>(() => service.DisableAsync(Guid.NewGuid()));
    }
}
