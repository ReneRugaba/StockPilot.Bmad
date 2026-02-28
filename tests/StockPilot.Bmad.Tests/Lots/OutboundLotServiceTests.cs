using Moq;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Tests.Lots;

public class OutboundLotServiceTests
{
    private static Lot MakeStoredLot(Guid? locationId = null)
    {
        var lot = Lot.CreateInbound(Guid.NewGuid(), locationId ?? Guid.NewGuid(), "REF-001", null, DateTime.UtcNow);
        return lot;
    }

    private static Location MakeOccupiedLocation()
    {
        var location = Location.Create(Guid.NewGuid(), "A-01", "Rack A1", DateTime.UtcNow);
        location.SetStatus(LocationStatus.Occupied, DateTime.UtcNow);
        return location;
    }

    private OutboundLotService MakeService(
        Mock<ILotRepository> lotRepo,
        Mock<ILocationRepository> locationRepo,
        Mock<IMovementRepository>? movementRepo = null)
    {
        movementRepo ??= new Mock<IMovementRepository>();
        movementRepo
            .Setup(r => r.CreateAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movement m, CancellationToken _) => m);

        lotRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        locationRepo
            .Setup(r => r.UpdateStatusAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new OutboundLotService(lotRepo.Object, movementRepo.Object, locationRepo.Object);
    }

    [Fact]
    public async Task OutboundAsync_EmptyLotId_ThrowsValidationException()
    {
        var lotRepo = new Mock<ILotRepository>();
        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(lotRepo, locationRepo);

        var request = new OutboundLotRequest { LotId = Guid.Empty };

        await Assert.ThrowsAsync<OutboundLotValidationException>(() => service.OutboundAsync(request));
    }

    [Fact]
    public async Task OutboundAsync_LotNotFound_ThrowsLotNotFoundException()
    {
        var lotRepo = new Mock<ILotRepository>();
        lotRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(lotRepo, locationRepo);

        var request = new OutboundLotRequest { LotId = Guid.NewGuid() };

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.OutboundAsync(request));
    }

    [Fact]
    public async Task OutboundAsync_LotNotStored_ThrowsValidationException()
    {
        var lot = MakeStoredLot();
        lot.Retrieve(DateTime.UtcNow); // passe en Retrieved

        var lotRepo = new Mock<ILotRepository>();
        lotRepo
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        var service = MakeService(lotRepo, locationRepo);

        var request = new OutboundLotRequest { LotId = lot.LotId };

        var ex = await Assert.ThrowsAsync<OutboundLotValidationException>(() => service.OutboundAsync(request));
        Assert.Contains("Stored", ex.Message);
    }

    [Fact]
    public async Task OutboundAsync_LocationNotOccupied_ThrowsValidationException()
    {
        var location = Location.Create(Guid.NewGuid(), "A-01", "Rack A1", DateTime.UtcNow);
        // location est AVAILABLE (pas OCCUPIED)

        var lot = MakeStoredLot(location.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var service = MakeService(lotRepo, locationRepo);

        var request = new OutboundLotRequest { LotId = lot.LotId };

        var ex = await Assert.ThrowsAsync<OutboundLotValidationException>(() => service.OutboundAsync(request));
        Assert.Contains("Occupied", ex.Message);
    }

    [Fact]
    public async Task OutboundAsync_ValidRequest_ReturnsLotDtoWithStatusRetrieved()
    {
        var location = MakeOccupiedLocation();
        var lot = MakeStoredLot(location.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var service = MakeService(lotRepo, locationRepo);

        var request = new OutboundLotRequest { LotId = lot.LotId, Notes = "Retrait client" };
        var result = await service.OutboundAsync(request);

        Assert.Equal(lot.LotId, result.LotId);
        Assert.Equal("RETRIEVED", result.Status);
        Assert.Equal(Guid.Empty, result.LocationId); // LocationId null => Guid.Empty dans le DTO
    }

    [Fact]
    public async Task OutboundAsync_ValidRequest_SetsLocationToAvailable()
    {
        var location = MakeOccupiedLocation();
        var lot = MakeStoredLot(location.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var service = MakeService(lotRepo, locationRepo);

        await service.OutboundAsync(new OutboundLotRequest { LotId = lot.LotId });

        locationRepo.Verify(r => r.UpdateStatusAsync(
            It.Is<Location>(l => l.Status == LocationStatus.Available),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OutboundAsync_ValidRequest_CreatesOutboundMovement()
    {
        var location = MakeOccupiedLocation();
        var lot = MakeStoredLot(location.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo
            .Setup(r => r.GetByIdAsync(location.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var movementRepo = new Mock<IMovementRepository>();
        movementRepo
            .Setup(r => r.CreateAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movement m, CancellationToken _) => m);

        var service = MakeService(lotRepo, locationRepo, movementRepo);

        await service.OutboundAsync(new OutboundLotRequest { LotId = lot.LotId });

        movementRepo.Verify(r => r.CreateAsync(
            It.Is<Movement>(m => m.Type == MovementType.Outbound && m.LotId == lot.LotId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

