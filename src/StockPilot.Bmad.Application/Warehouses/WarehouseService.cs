using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Application.Warehouses;

public class WarehouseService
{
    private readonly IWarehouseRepository _repository;

    public WarehouseService(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new WarehouseValidationException("Warehouse name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            throw new WarehouseValidationException("Warehouse address is required.");
        }

        var now = DateTime.UtcNow;

        var warehouse = Warehouse.Create(request.Name, request.Address, now);

        warehouse = await _repository.CreateAsync(warehouse, cancellationToken);

        return ToDto(warehouse);
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await _repository.GetAllAsync(cancellationToken);
        return warehouses.Select(ToDto).ToList();
    }

    public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(id, cancellationToken);
        return warehouse is null ? null : ToDto(warehouse);
    }

    private static WarehouseDto ToDto(Warehouse warehouse) => new()
    {
        WarehouseId = warehouse.WarehouseId,
        Name = warehouse.Name,
        Address = warehouse.Address,
        Status = warehouse.Status.ToString().ToUpperInvariant(),
        CreatedAt = warehouse.CreatedAt,
        UpdatedAt = warehouse.UpdatedAt
    };
}

