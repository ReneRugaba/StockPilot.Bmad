using Moq;
using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Domain.Lots;
using Xunit;

namespace StockPilot.Bmad.Tests.Lots;

public class UpdateLotServiceTests
{
    [Fact]
    public async Task UpdateLot_Stored_UpdatesReferenceAndDescription()
    {
        var now = DateTime.UtcNow;
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", "Old", now);

        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        repoMock
            .Setup(r => r.UpdateAsync(lot, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UpdateLotService(repoMock.Object);

        var result = await service.UpdateAsync(lot.LotId, new UpdateLotRequest { Reference = "REF-002", Description = "New" });

        Assert.Equal("REF-002", result.Reference);
        Assert.Equal("New", result.Description);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLot_InTransit_Returns400ViaValidationException()
    {
        var now = DateTime.UtcNow;
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", null, now);
        lot.SetStatus(LotStatus.InTransit, now);

        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = new UpdateLotService(repoMock.Object);

        await Assert.ThrowsAsync<LotUpdateValidationException>(() => service.UpdateAsync(lot.LotId, new UpdateLotRequest { Reference = "REF-002" }));
    }

    [Fact]
    public async Task UpdateLot_Retrieved_Returns400ViaValidationException()
    {
        var now = DateTime.UtcNow;
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", null, now);
        lot.Retrieve(now);

        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = new UpdateLotService(repoMock.Object);

        await Assert.ThrowsAsync<LotUpdateValidationException>(() => service.UpdateAsync(lot.LotId, new UpdateLotRequest { Reference = "REF-002" }));
    }

    [Fact]
    public async Task UpdateLot_NotFound_ThrowsLotNotFound()
    {
        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lot?)null);

        var service = new UpdateLotService(repoMock.Object);

        await Assert.ThrowsAsync<LotNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), new UpdateLotRequest { Reference = "REF" }));
    }

    [Fact]
    public async Task ArchiveLot_Retrieved_SetsArchived()
    {
        var now = DateTime.UtcNow;
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", null, now);
        lot.Retrieve(now);

        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);
        repoMock
            .Setup(r => r.UpdateAsync(lot, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UpdateLotService(repoMock.Object);

        await service.ArchiveAsync(lot.LotId);

        Assert.Equal(LotStatus.Archived, lot.Status);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveLot_Stored_ThrowsValidationException()
    {
        var now = DateTime.UtcNow;
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", null, now);

        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = new UpdateLotService(repoMock.Object);

        await Assert.ThrowsAsync<LotUpdateValidationException>(() => service.ArchiveAsync(lot.LotId));
    }

    [Fact]
    public async Task ArchiveLot_AlreadyArchived_IsIdempotent_DoesNotPersist()
    {
        var now = DateTime.UtcNow;
        var lot = Lot.CreateInbound(Guid.NewGuid(), Guid.NewGuid(), "REF-001", null, now);
        lot.SetStatus(LotStatus.Archived, now);

        var repoMock = new Mock<ILotRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(lot.LotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        var service = new UpdateLotService(repoMock.Object);

        await service.ArchiveAsync(lot.LotId);

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

