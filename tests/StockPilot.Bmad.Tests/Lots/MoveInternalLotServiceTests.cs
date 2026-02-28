using Moq;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Tests.Lots;

public class MoveInternalLotServiceTests
{
    private static readonly Guid WarehouseId = Guid.NewGuid();

    private static Lot MakeStoredLot(Guid sourceLocationId)
        => Lot.CreateInbound(Guid.NewGuid(), sourceLocationId, "REF-001", null, DateTime.UtcNow);

    private static Location MakeOccupiedLocation(Guid? warehouseId = null)
    {
        var loc = Location.Create(warehouseId ?? WarehouseId, "A-01", "Rack A1", DateTime.UtcNow);
        loc.SetStatus(LocationStatus.Occupied, DateTime.UtcNow);
        return loc;
    }

    private static Location MakeAvailableLocation(Guid? warehouseId = null)
        => Location.Create(warehouseId ?? WarehouseId, "B-01", "Rack B1", DateTime.UtcNow);

    private MoveInternalLotService MakeService(
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

        return new MoveInternalLotService(lotRepo.Object, locationRepo.Object, movementRepo.Object);
    }

    [Fact]
    public async Task MoveAsync_EmptyLotId_ThrowsValidationException()
    {
        var service = MakeService(new Mock<ILotRepository>(), new Mock<ILocationRepository>());
        var request = new MoveInternalLotRequest { LotId = Guid.Empty, DestinationLocationId = Guid.NewGuid() };

        await Assert.ThrowsAsync<MoveInternalLotValidationException>(() => service.MoveAsync(request));
    }

    [Fact]
    public async Task MoveAsync_EmptyDestinationId_ThrowsValidationException()
    {
        var service = MakeService(new Mock<ILotRepository>(), new Mock<ILocationRepository>());
        var request = new MoveInternalLotRequest { LotId = Guid.NewGuid(), DestinationLocationId = Guid.Empty };

        await Assert.ThrowsAsync<MoveInternalLotValidationException>(() => service.MoveAsync(request));
    }

    [Fact]
    public async Task MoveAsync_LotNotFound_ThrowsLotNotFoundException()
    {
        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var service = MakeService(lotRepo, new Mock<ILocationRepository>());
        var request = new MoveInternalLotRequest { LotId = Guid.NewGuid(), DestinationLocationId = Guid.NewGuid() };

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.MoveAsync(request));
    }

    [Fact]
    public async Task MoveAsync_LotNotStored_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation();
        var lot = MakeStoredLot(source.LocationId);
        lot.Retrieve(DateTime.UtcNow); // passe en Retrieved

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = MakeService(lotRepo, new Mock<ILocationRepository>());
        var request = new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = Guid.NewGuid() };

        var ex = await Assert.ThrowsAsync<MoveInternalLotValidationException>(() => service.MoveAsync(request));
        Assert.Contains("Stored", ex.Message);
    }

    [Fact]
    public async Task MoveAsync_DestinationNotFound_ThrowsLotNotFoundException()
    {
        var source = MakeOccupiedLocation();
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = MakeService(lotRepo, locationRepo);
        var request = new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = Guid.NewGuid() };

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.MoveAsync(request));
    }

    [Fact]
    public async Task MoveAsync_DestinationOccupied_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation();
        var destination = MakeOccupiedLocation(); // aussi OCCUPIED
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var service = MakeService(lotRepo, locationRepo);
        var request = new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId };

        var ex = await Assert.ThrowsAsync<MoveInternalLotValidationException>(() => service.MoveAsync(request));
        Assert.Contains("available", ex.Message);
    }

    [Fact]
    public async Task MoveAsync_DifferentWarehouses_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation(WarehouseId);
        var destination = MakeAvailableLocation(Guid.NewGuid()); // entrepôt différent
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);
        locationRepo.Setup(r => r.GetByIdAsync(source.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);

        var service = MakeService(lotRepo, locationRepo);
        var request = new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId };

        var ex = await Assert.ThrowsAsync<MoveInternalLotValidationException>(() => service.MoveAsync(request));
        Assert.Contains("warehouse", ex.Message);
    }

    [Fact]
    public async Task MoveAsync_ValidRequest_ReturnsLotDtoWithNewLocation()
    {
        var source = MakeOccupiedLocation();
        var destination = MakeAvailableLocation();
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);
        locationRepo.Setup(r => r.GetByIdAsync(source.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);

        var service = MakeService(lotRepo, locationRepo);
        var request = new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId };

        var result = await service.MoveAsync(request);

        Assert.Equal(lot.LotId, result.LotId);
        Assert.Equal(destination.LocationId, result.LocationId);
        Assert.Equal("STORED", result.Status);
    }

    [Fact]
    public async Task MoveAsync_ValidRequest_CreatesInternalMoveMovement()
    {
        var source = MakeOccupiedLocation();
        var destination = MakeAvailableLocation();
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);
        locationRepo.Setup(r => r.GetByIdAsync(source.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);

        var movementRepo = new Mock<IMovementRepository>();
        movementRepo
            .Setup(r => r.CreateAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movement m, CancellationToken _) => m);

        var service = MakeService(lotRepo, locationRepo, movementRepo);
        await service.MoveAsync(new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId });

        movementRepo.Verify(r => r.CreateAsync(
            It.Is<Movement>(m => m.Type == MovementType.InternalMove
                              && m.LotId == lot.LotId
                              && m.FromLocationId == source.LocationId
                              && m.ToLocationId == destination.LocationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveAsync_ValidRequest_UpdatesSourceToAvailableAndDestinationToOccupied()
    {
        var source = MakeOccupiedLocation();
        var destination = MakeAvailableLocation();
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);
        locationRepo.Setup(r => r.GetByIdAsync(source.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);

        var service = MakeService(lotRepo, locationRepo);
        await service.MoveAsync(new MoveInternalLotRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId });

        locationRepo.Verify(r => r.UpdateStatusAsync(
            It.Is<Location>(l => l.LocationId == source.LocationId && l.Status == LocationStatus.Available),
            It.IsAny<CancellationToken>()), Times.Once);

        locationRepo.Verify(r => r.UpdateStatusAsync(
            It.Is<Location>(l => l.LocationId == destination.LocationId && l.Status == LocationStatus.Occupied),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

