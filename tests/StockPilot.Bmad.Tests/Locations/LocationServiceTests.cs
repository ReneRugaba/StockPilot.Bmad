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
}

