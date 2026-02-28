using Moq;
using StockPilot.Bmad.Application.Locations;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Tests.Lots;

public class TransferLotServiceTests
{
    private static readonly Guid WarehouseA = Guid.NewGuid();
    private static readonly Guid WarehouseB = Guid.NewGuid();

    private static Lot MakeStoredLot(Guid sourceLocationId)
        => Lot.CreateInbound(Guid.NewGuid(), sourceLocationId, "REF-001", null, DateTime.UtcNow);

    private static Location MakeOccupiedLocation(Guid warehouseId)
    {
        var loc = Location.Create(warehouseId, "A-01", "Rack A1", DateTime.UtcNow);
        loc.SetStatus(LocationStatus.Occupied, DateTime.UtcNow);
        return loc;
    }

    private static Location MakeAvailableLocation(Guid warehouseId)
        => Location.Create(warehouseId, "B-01", "Rack B1", DateTime.UtcNow);

    private TransferLotService MakeService(
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

        return new TransferLotService(lotRepo.Object, locationRepo.Object, movementRepo.Object);
    }

    // --- DISPATCH ---

    [Fact]
    public async Task DispatchAsync_EmptyLotId_ThrowsValidationException()
    {
        var service = MakeService(new Mock<ILotRepository>(), new Mock<ILocationRepository>());
        await Assert.ThrowsAsync<TransferLotValidationException>(() =>
            service.DispatchAsync(new TransferDispatchRequest { LotId = Guid.Empty, DestinationLocationId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task DispatchAsync_LotNotFound_ThrowsLotNotFoundException()
    {
        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var service = MakeService(lotRepo, new Mock<ILocationRepository>());
        await Assert.ThrowsAsync<LotNotFoundException>(() =>
            service.DispatchAsync(new TransferDispatchRequest { LotId = Guid.NewGuid(), DestinationLocationId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task DispatchAsync_LotNotStored_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId);
        lot.Retrieve(DateTime.UtcNow); // passe en Retrieved

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = MakeService(lotRepo, new Mock<ILocationRepository>());
        var ex = await Assert.ThrowsAsync<TransferLotValidationException>(() =>
            service.DispatchAsync(new TransferDispatchRequest { LotId = lot.LotId, DestinationLocationId = Guid.NewGuid() }));
        Assert.Contains("Stored", ex.Message);
    }

    [Fact]
    public async Task DispatchAsync_DestinationNotFound_ThrowsLotNotFoundException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = MakeService(lotRepo, locationRepo);
        await Assert.ThrowsAsync<LotNotFoundException>(() =>
            service.DispatchAsync(new TransferDispatchRequest { LotId = lot.LotId, DestinationLocationId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task DispatchAsync_DestinationOccupied_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var destination = MakeOccupiedLocation(WarehouseB);
        var lot = MakeStoredLot(source.LocationId);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var service = MakeService(lotRepo, locationRepo);
        var ex = await Assert.ThrowsAsync<TransferLotValidationException>(() =>
            service.DispatchAsync(new TransferDispatchRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId }));
        Assert.Contains("available", ex.Message);
    }

    [Fact]
    public async Task DispatchAsync_SameWarehouse_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var destination = MakeAvailableLocation(WarehouseA); // même entrepôt
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
        var ex = await Assert.ThrowsAsync<TransferLotValidationException>(() =>
            service.DispatchAsync(new TransferDispatchRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId }));
        Assert.Contains("different warehouses", ex.Message);
    }

    [Fact]
    public async Task DispatchAsync_ValidRequest_ReturnsLotInTransit()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var destination = MakeAvailableLocation(WarehouseB);
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
        var result = await service.DispatchAsync(new TransferDispatchRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId });

        Assert.Equal("INTRANSIT", result.Status);
        Assert.Equal(Guid.Empty, result.LocationId);
    }

    [Fact]
    public async Task DispatchAsync_ValidRequest_CreatesTransferDispatchMovement()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var destination = MakeAvailableLocation(WarehouseB);
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
        movementRepo.Setup(r => r.CreateAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movement m, CancellationToken _) => m);

        var service = MakeService(lotRepo, locationRepo, movementRepo);
        await service.DispatchAsync(new TransferDispatchRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId });

        movementRepo.Verify(r => r.CreateAsync(
            It.Is<Movement>(m => m.Type == MovementType.Transfer
                              && m.FromLocationId == source.LocationId
                              && m.ToLocationId == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- RECEIVE ---

    [Fact]
    public async Task ReceiveAsync_LotNotFound_ThrowsLotNotFoundException()
    {
        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var service = MakeService(lotRepo, new Mock<ILocationRepository>());
        await Assert.ThrowsAsync<LotNotFoundException>(() =>
            service.ReceiveAsync(new TransferReceiveRequest { LotId = Guid.NewGuid(), DestinationLocationId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task ReceiveAsync_LotNotInTransit_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId); // STORED, pas IN_TRANSIT

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = MakeService(lotRepo, new Mock<ILocationRepository>());
        var ex = await Assert.ThrowsAsync<TransferLotValidationException>(() =>
            service.ReceiveAsync(new TransferReceiveRequest { LotId = lot.LotId, DestinationLocationId = Guid.NewGuid() }));
        Assert.Contains("InTransit", ex.Message);
    }

    [Fact]
    public async Task ReceiveAsync_DestinationNotFound_ThrowsLotNotFoundException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId);
        lot.Dispatch(DateTime.UtcNow);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        var service = MakeService(lotRepo, locationRepo);
        await Assert.ThrowsAsync<LotNotFoundException>(() =>
            service.ReceiveAsync(new TransferReceiveRequest { LotId = lot.LotId, DestinationLocationId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task ReceiveAsync_DestinationOccupied_ThrowsValidationException()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId);
        lot.Dispatch(DateTime.UtcNow);

        var destination = MakeOccupiedLocation(WarehouseB);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var service = MakeService(lotRepo, locationRepo);
        var ex = await Assert.ThrowsAsync<TransferLotValidationException>(() =>
            service.ReceiveAsync(new TransferReceiveRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId }));
        Assert.Contains("available", ex.Message);
    }

    [Fact]
    public async Task ReceiveAsync_ValidRequest_ReturnsLotStored()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId);
        lot.Dispatch(DateTime.UtcNow);

        var destination = MakeAvailableLocation(WarehouseB);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var service = MakeService(lotRepo, locationRepo);
        var result = await service.ReceiveAsync(new TransferReceiveRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId });

        Assert.Equal("STORED", result.Status);
        Assert.Equal(destination.LocationId, result.LocationId);
    }

    [Fact]
    public async Task ReceiveAsync_ValidRequest_CreatesTransferReceiveMovement()
    {
        var source = MakeOccupiedLocation(WarehouseA);
        var lot = MakeStoredLot(source.LocationId);
        lot.Dispatch(DateTime.UtcNow);

        var destination = MakeAvailableLocation(WarehouseB);

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var locationRepo = new Mock<ILocationRepository>();
        locationRepo.Setup(r => r.GetByIdAsync(destination.LocationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var movementRepo = new Mock<IMovementRepository>();
        movementRepo.Setup(r => r.CreateAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movement m, CancellationToken _) => m);

        var service = MakeService(lotRepo, locationRepo, movementRepo);
        await service.ReceiveAsync(new TransferReceiveRequest { LotId = lot.LotId, DestinationLocationId = destination.LocationId });

        movementRepo.Verify(r => r.CreateAsync(
            It.Is<Movement>(m => m.Type == MovementType.Transfer
                              && m.FromLocationId == null
                              && m.ToLocationId == destination.LocationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

