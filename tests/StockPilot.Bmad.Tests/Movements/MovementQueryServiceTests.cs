using Moq;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;
using StockPilot.Bmad.Domain.Lots;

namespace StockPilot.Bmad.Tests.Movements;

public class MovementQueryServiceTests
{
    private static Lot MakeStoredLot()
        => Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", null, DateTime.UtcNow);

    private static MovementDto MakeMovementDto(Guid lotId) => new()
    {
        MovementId = Guid.NewGuid(),
        LotId = lotId,
        Type = "INBOUND",
        FromLocationId = null,
        ToLocationId = Guid.NewGuid(),
        OccurredAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByLotIdAsync_ExistingLot_ReturnsMovements()
    {
        var lot = MakeStoredLot();
        var movements = new List<MovementDto> { MakeMovementDto(lot.LotId), MakeMovementDto(lot.LotId) };

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var movementRepo = new Mock<IMovementRepository>();
        movementRepo.Setup(r => r.GetByLotIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movements);

        var service = new MovementQueryService(movementRepo.Object, lotRepo.Object);
        var result = await service.GetByLotIdAsync(lot.LotId);

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(lot.LotId, m.LotId));
    }

    [Fact]
    public async Task GetByLotIdAsync_LotNotFound_ThrowsLotNotFoundException()
    {
        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var movementRepo = new Mock<IMovementRepository>();
        var service = new MovementQueryService(movementRepo.Object, lotRepo.Object);

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.GetByLotIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByLotIdAsync_EmptyLotId_ThrowsLotNotFoundException()
    {
        var lotRepo = new Mock<ILotRepository>();
        var movementRepo = new Mock<IMovementRepository>();
        var service = new MovementQueryService(movementRepo.Object, lotRepo.Object);

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.GetByLotIdAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetByLotIdAsync_NoMovements_ReturnsEmptyList()
    {
        var lot = MakeStoredLot();

        var lotRepo = new Mock<ILotRepository>();
        lotRepo.Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var movementRepo = new Mock<IMovementRepository>();
        movementRepo.Setup(r => r.GetByLotIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MovementDto>());

        var service = new MovementQueryService(movementRepo.Object, lotRepo.Object);
        var result = await service.GetByLotIdAsync(lot.LotId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingMovement_ReturnsMovementDto()
    {
        var lot = MakeStoredLot();
        var dto = MakeMovementDto(lot.LotId);

        var movementRepo = new Mock<IMovementRepository>();
        movementRepo.Setup(r => r.GetByIdAsync(dto.MovementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var lotRepo = new Mock<ILotRepository>();
        var service = new MovementQueryService(movementRepo.Object, lotRepo.Object);

        var result = await service.GetByIdAsync(dto.MovementId);

        Assert.Equal(dto.MovementId, result.MovementId);
        Assert.Equal("INBOUND", result.Type);
    }

    [Fact]
    public async Task GetByIdAsync_MovementNotFound_ThrowsMovementNotFoundException()
    {
        var movementRepo = new Mock<IMovementRepository>();
        movementRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovementDto?)null);

        var lotRepo = new Mock<ILotRepository>();
        var service = new MovementQueryService(movementRepo.Object, lotRepo.Object);

        await Assert.ThrowsAsync<MovementNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }
}

