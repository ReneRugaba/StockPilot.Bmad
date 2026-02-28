using StockPilot.Bmad.Domain.Lots;

namespace StockPilot.Bmad.Application.Lots;

public class UpdateLotService
{
    private readonly ILotRepository _repository;

    public UpdateLotService(ILotRepository repository)
    {
        _repository = repository;
    }

    public async Task<LotDto> UpdateAsync(Guid lotId, UpdateLotRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reference))
            throw new LotUpdateValidationException("Lot reference is required.");

        var lot = await _repository.GetByIdAsync(lotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(lotId);

        if (lot.Status is LotStatus.InTransit or LotStatus.Retrieved)
            throw new LotUpdateValidationException($"Cannot update a lot with status '{lot.Status.ToString().ToUpperInvariant()}'.");

        lot.UpdateMetadata(request.Reference, request.Description, DateTime.UtcNow);
        await _repository.UpdateAsync(lot, cancellationToken);

        return new LotDto
        {
            LotId = lot.LotId,
            ClientId = lot.ClientId,
            LocationId = lot.LocationId,
            Reference = lot.Reference,
            Description = lot.Description,
            Status = lot.Status.ToString().ToUpperInvariant(),
            CreatedAt = lot.CreatedAt,
            UpdatedAt = lot.UpdatedAt
        };
    }

    public async Task ArchiveAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        var lot = await _repository.GetByIdAsync(lotId, cancellationToken);
        if (lot is null)
            throw new LotNotFoundException(lotId);

        // Idempotent
        if (lot.Status == LotStatus.Archived)
            return;

        if (lot.Status == LotStatus.Stored)
            throw new LotUpdateValidationException("Cannot archive a lot with status 'STORED'. Retrieve it first.");

        lot.Archive(DateTime.UtcNow);
        await _repository.UpdateAsync(lot, cancellationToken);
    }
}

