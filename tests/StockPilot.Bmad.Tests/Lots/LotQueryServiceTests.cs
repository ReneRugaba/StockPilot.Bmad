using Moq;
using StockPilot.Bmad.Application.Lots;

namespace StockPilot.Bmad.Tests.Lots;

public class LotQueryServiceTests
{
    private static LotDetailDto MakeLotDetail(Guid? lotId = null) => new()
    {
        LotId = lotId ?? Guid.NewGuid(),
        Reference = "REF-001",
        Status = "STORED",
        ClientId = Guid.NewGuid(),
        ClientName = "Client Test",
        LocationId = Guid.NewGuid(),
        LocationCode = "A-01",
        WarehouseId = Guid.NewGuid(),
        WarehouseName = "Entrepôt Test"
    };

    [Fact]
    public async Task GetAllAsync_ReturnsList()
    {
        var lots = new List<LotDetailDto> { MakeLotDetail(), MakeLotDetail() };

        var repo = new Mock<ILotRepository>();
        repo.Setup(r => r.GetAllDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);

        var service = new LotQueryService(repo.Object);
        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLot_ReturnsLotDetail()
    {
        var lot = MakeLotDetail();

        var repo = new Mock<ILotRepository>();
        repo.Setup(r => r.GetDetailByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = new LotQueryService(repo.Object);
        var result = await service.GetByIdAsync(lot.LotId);

        Assert.Equal(lot.LotId, result.LotId);
        Assert.Equal("REF-001", result.Reference);
        Assert.Equal("Client Test", result.ClientName);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownLot_ThrowsLotNotFoundException()
    {
        var repo = new Mock<ILotRepository>();
        repo.Setup(r => r.GetDetailByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LotDetailDto?)null);

        var service = new LotQueryService(repo.Object);

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_EmptyGuid_ThrowsLotNotFoundException()
    {
        var repo = new Mock<ILotRepository>();
        var service = new LotQueryService(repo.Object);

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.GetByIdAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsLotsForClient()
    {
        var clientId = Guid.NewGuid();
        var lots = new List<LotDetailDto>
        {
            new() { LotId = Guid.NewGuid(), ClientId = clientId, ClientName = "Client A", Reference = "REF-001", Status = "STORED", LocationId = Guid.NewGuid(), LocationCode = "A-01", WarehouseId = Guid.NewGuid(), WarehouseName = "W1" },
            new() { LotId = Guid.NewGuid(), ClientId = clientId, ClientName = "Client A", Reference = "REF-002", Status = "STORED", LocationId = Guid.NewGuid(), LocationCode = "A-02", WarehouseId = Guid.NewGuid(), WarehouseName = "W1" }
        };

        var repo = new Mock<ILotRepository>();
        repo.Setup(r => r.GetDetailsByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);

        var service = new LotQueryService(repo.Object);
        var result = await service.GetByClientIdAsync(clientId);

        Assert.Equal(2, result.Count);
        Assert.All(result, l => Assert.Equal(clientId, l.ClientId));
    }

    [Fact]
    public async Task GetByWarehouseIdAsync_ReturnsLotsForWarehouse()
    {
        var warehouseId = Guid.NewGuid();
        var lots = new List<LotDetailDto>
        {
            new() { LotId = Guid.NewGuid(), WarehouseId = warehouseId, WarehouseName = "W1", Reference = "REF-001", Status = "STORED", ClientId = Guid.NewGuid(), ClientName = "C1", LocationId = Guid.NewGuid(), LocationCode = "A-01" }
        };

        var repo = new Mock<ILotRepository>();
        repo.Setup(r => r.GetDetailsByWarehouseIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);

        var service = new LotQueryService(repo.Object);
        var result = await service.GetByWarehouseIdAsync(warehouseId);

        Assert.Single(result);
        Assert.Equal(warehouseId, result[0].WarehouseId);
    }
}

