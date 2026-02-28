using System.Net.Http.Json;

namespace StockPilot.Bmad.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // --- Clients ---
    public Task<List<ClientDto>?> GetClientsAsync()
        => _http.GetFromJsonAsync<List<ClientDto>>("clients");

    public Task<ClientDto?> GetClientAsync(Guid id)
        => _http.GetFromJsonAsync<ClientDto>($"clients/{id}");

    // --- Warehouses ---
    public Task<List<WarehouseDto>?> GetWarehousesAsync()
        => _http.GetFromJsonAsync<List<WarehouseDto>>("warehouses");

    public Task<WarehouseDto?> GetWarehouseAsync(Guid id)
        => _http.GetFromJsonAsync<WarehouseDto>($"warehouses/{id}");

    // --- Locations ---
    public Task<List<LocationDto>?> GetLocationsAsync()
        => _http.GetFromJsonAsync<List<LocationDto>>("locations");

    public Task<List<LocationDto>?> GetLocationsByWarehouseAsync(Guid warehouseId)
        => _http.GetFromJsonAsync<List<LocationDto>>($"warehouses/{warehouseId}/locations");

    // --- Lots ---
    public Task<List<LotDetailDto>?> GetLotsAsync()
        => _http.GetFromJsonAsync<List<LotDetailDto>>("lots");

    public Task<LotDetailDto?> GetLotAsync(Guid id)
        => _http.GetFromJsonAsync<LotDetailDto>($"lots/{id}");

    // --- Movements ---
    public Task<List<MovementDto>?> GetMovementsByLotAsync(Guid lotId)
        => _http.GetFromJsonAsync<List<MovementDto>>($"lots/{lotId}/movements");
}

// DTOs locaux pour la désérialisation JSON
public class ClientDto
{
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class WarehouseDto
{
    public Guid WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LocationDto
{
    public Guid LocationId { get; set; }
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LotDetailDto
{
    public Guid LotId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}

public class MovementDto
{
    public Guid MovementId { get; set; }
    public Guid LotId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Reason { get; set; }
}

