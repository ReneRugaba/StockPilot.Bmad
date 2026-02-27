using Moq;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Clients;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;
using Xunit;

namespace StockPilot.Bmad.Tests.Lots;

public class InboundLotServiceTests
{
    private static Client MakeClient()
        => Client.Create("Test Client", "test@test.com", DateTime.UtcNow);

    private static Location MakeAvailableLocation()
        => Location.Create(Guid.NewGuid(), "A-01", "Rack A1", DateTime.UtcNow);

    private InboundLotService MakeService(
        Mock<IClientRepository> clientRepo,
        Mock<ILocationRepository> locationRepo,
        Mock<ILotRepository>? lotRepo = null,
        Mock<IMovementRepository>? movementRepo = null)
    {
        lotRepo ??= new Mock<ILotRepository>();
        movementRepo ??= new Mock<IMovementRepository>();

        lotRepo
            .Setup(r => r.CreateAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot l, CancellationToken _) => l);

        movementRepo
            .Setup(r => r.CreateAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movement m, CancellationToken _) => m);

        locationRepo
            .Setup(r => r.UpdateStatusAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new InboundLotService(lotRepo.Object, movementRepo.Object, clientRepo.Object, locationRepo.Object);
    }

    [Fact]
    public async Task InboundAsync_ValidRequest_ReturnsLotDtoWithStatusStored()
    {
        var client = MakeClient();
        var location = MakeAvailableLocation();

        var clientRepo = new Mock<IClientRepository>();
        clientRepo
            .Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = client.ClientId,
            LocationId = location.LocationId,
            Reference = "REF-TEST",
            Description = "Une description"
        };

        var result = await service.InboundAsync(request);

        Assert.NotEqual(Guid.Empty, result.LotId);
        Assert.Equal(client.ClientId, result.ClientId);
        Assert.Equal(location.LocationId, result.LocationId);
        Assert.Equal("REF-TEST", result.Reference);
        Assert.Equal("Une description", result.Description);
        Assert.Equal("STORED", result.Status);
    }

    [Fact]
    public async Task InboundAsync_EmptyClientId_ThrowsValidationException()
    {
        var clientRepo = new Mock<IClientRepository>();
        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = Guid.Empty,
            LocationId = Guid.NewGuid(),
            Reference = "REF-001"
        };

        await Assert.ThrowsAsync<InboundLotValidationException>(() => service.InboundAsync(request));
    }

    [Fact]
    public async Task InboundAsync_EmptyLocationId_ThrowsValidationException()
    {
        var clientRepo = new Mock<IClientRepository>();
        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = Guid.NewGuid(),
            LocationId = Guid.Empty,
            Reference = "REF-001"
        };

        await Assert.ThrowsAsync<InboundLotValidationException>(() => service.InboundAsync(request));
    }

    [Fact]
    public async Task InboundAsync_EmptyReference_ThrowsValidationException()
    {
        var clientRepo = new Mock<IClientRepository>();
        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = Guid.NewGuid(),
            LocationId = Guid.NewGuid(),
            Reference = " "
        };

        await Assert.ThrowsAsync<InboundLotValidationException>(() => service.InboundAsync(request));
    }

    [Fact]
    public async Task InboundAsync_ClientNotFound_ThrowsValidationException()
    {
        var clientRepo = new Mock<IClientRepository>();
        clientRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = Guid.NewGuid(),
            LocationId = Guid.NewGuid(),
            Reference = "REF-001"
        };

        var ex = await Assert.ThrowsAsync<InboundLotValidationException>(() => service.InboundAsync(request));
        Assert.Contains("Client", ex.Message);
    }

    [Fact]
    public async Task InboundAsync_LocationNotFound_ThrowsValidationException()
    {
        var client = MakeClient();

        var clientRepo = new Mock<IClientRepository>();
        clientRepo
            .Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = client.ClientId,
            LocationId = Guid.NewGuid(),
            Reference = "REF-001"
        };

        var ex = await Assert.ThrowsAsync<InboundLotValidationException>(() => service.InboundAsync(request));
        Assert.Contains("Location", ex.Message);
    }

    [Fact]
    public async Task InboundAsync_LocationOccupied_ThrowsValidationException()
    {
        var client = MakeClient();
        var location = MakeAvailableLocation();
        location.SetStatus(LocationStatus.Occupied, DateTime.UtcNow);

        var clientRepo = new Mock<IClientRepository>();
        clientRepo
            .Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = client.ClientId,
            LocationId = location.LocationId,
            Reference = "REF-001"
        };

        var ex = await Assert.ThrowsAsync<InboundLotValidationException>(() => service.InboundAsync(request));
        Assert.Contains("not available", ex.Message);
    }

    [Fact]
    public async Task InboundAsync_ValidRequest_UpdatesLocationStatusToOccupied()
    {
        var client = MakeClient();
        var location = MakeAvailableLocation();

        var clientRepo = new Mock<IClientRepository>();
        clientRepo
            .Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);
        locationRepo
            .Setup(r => r.UpdateStatusAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = MakeService(clientRepo, locationRepo);

        var request = new InboundLotRequest
        {
            ClientId = client.ClientId,
            LocationId = location.LocationId,
            Reference = "REF-001"
        };

        await service.InboundAsync(request);

        locationRepo.Verify(r => r.UpdateStatusAsync(
            It.Is<Location>(l => l.Status == LocationStatus.Occupied),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

