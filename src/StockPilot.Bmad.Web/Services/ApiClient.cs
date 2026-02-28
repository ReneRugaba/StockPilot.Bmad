using System.Net;
using System.Net.Http.Json;

namespace StockPilot.Bmad.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string? body = null;
        try { body = await response.Content.ReadAsStringAsync(); } catch { /* ignore */ }

        var status = (int)response.StatusCode;
        var reason = response.ReasonPhrase ?? response.StatusCode.ToString();

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new ApiException(status, "Ressource introuvable (404).", body);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new ApiException(status, "Requête invalide (400).", body);

        throw new ApiException(status, $"Erreur API ({status}) : {reason}", body);
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await _http.PostAsJsonAsync(url, request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    private async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await _http.PutAsJsonAsync(url, request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    private async Task DeleteAsync(string url)
    {
        var response = await _http.DeleteAsync(url);
        await EnsureSuccessAsync(response);
    }

    // --- Clients ---
    public Task<List<ClientDto>?> GetClientsAsync() => _http.GetFromJsonAsync<List<ClientDto>>("clients");
    public Task<ClientDto?> GetClientAsync(Guid id) => _http.GetFromJsonAsync<ClientDto>($"clients/{id}");
    public Task<ClientDto?> CreateClientAsync(CreateClientRequest request) => PostAsync<CreateClientRequest, ClientDto>("clients", request);
    public Task<ClientDto?> UpdateClientAsync(Guid id, UpdateClientRequest request) => PutAsync<UpdateClientRequest, ClientDto>($"clients/{id}", request);
    public Task DisableClientAsync(Guid id) => DeleteAsync($"clients/{id}");

    // --- Warehouses ---
    public Task<List<WarehouseDto>?> GetWarehousesAsync() => _http.GetFromJsonAsync<List<WarehouseDto>>("warehouses");
    public Task<WarehouseDto?> GetWarehouseAsync(Guid id) => _http.GetFromJsonAsync<WarehouseDto>($"warehouses/{id}");
    public Task<WarehouseDto?> CreateWarehouseAsync(CreateWarehouseRequest request) => PostAsync<CreateWarehouseRequest, WarehouseDto>("warehouses", request);
    public Task<WarehouseDto?> UpdateWarehouseAsync(Guid id, UpdateWarehouseRequest request) => PutAsync<UpdateWarehouseRequest, WarehouseDto>($"warehouses/{id}", request);
    public Task CloseWarehouseAsync(Guid id) => DeleteAsync($"warehouses/{id}");

    // --- Locations ---
    public Task<List<LocationDto>?> GetLocationsAsync() => _http.GetFromJsonAsync<List<LocationDto>>("locations");
    public Task<LocationDto?> GetLocationAsync(Guid id) => _http.GetFromJsonAsync<LocationDto>($"locations/{id}");
    public Task<List<LocationDto>?> GetLocationsByWarehouseAsync(Guid warehouseId) => _http.GetFromJsonAsync<List<LocationDto>>($"warehouses/{warehouseId}/locations");
    public Task<LocationDto?> CreateLocationAsync(CreateLocationRequest request) => PostAsync<CreateLocationRequest, LocationDto>("locations", request);
    public Task<LocationDto?> UpdateLocationAsync(Guid id, UpdateLocationRequest request) => PutAsync<UpdateLocationRequest, LocationDto>($"locations/{id}", request);
    public Task DisableLocationAsync(Guid id) => DeleteAsync($"locations/{id}");

    // --- Lots ---
    public Task<List<LotDetailDto>?> GetLotsAsync() => _http.GetFromJsonAsync<List<LotDetailDto>>("lots");
    public Task<List<LotDetailDto>?> GetLotsByClientAsync(Guid clientId) => _http.GetFromJsonAsync<List<LotDetailDto>>($"clients/{clientId}/lots");
    public Task<List<LotDetailDto>?> GetLotsByWarehouseAsync(Guid warehouseId) => _http.GetFromJsonAsync<List<LotDetailDto>>($"warehouses/{warehouseId}/lots");
    public Task<LotDetailDto?> GetLotAsync(Guid id) => _http.GetFromJsonAsync<LotDetailDto>($"lots/{id}");
    public Task<LotDto?> UpdateLotAsync(Guid id, UpdateLotRequest request) => PutAsync<UpdateLotRequest, LotDto>($"lots/{id}", request);
    public Task ArchiveLotAsync(Guid id) => DeleteAsync($"lots/{id}");
    public Task<LotDto?> CreateInboundLotAsync(InboundLotRequest request) => PostAsync<InboundLotRequest, LotDto>("lots/inbound", request);
    public Task<LotDto?> RegisterOutboundAsync(Guid lotId, string? notes) => PostAsync<OutboundLotRequest, LotDto>("lots/outbound", new OutboundLotRequest { LotId = lotId, Notes = notes });
    public Task<LotDto?> MoveLotAsync(Guid lotId, Guid destinationLocationId, string? notes) => PostAsync<MoveLotRequest, LotDto>("lots/move", new MoveLotRequest { LotId = lotId, DestinationLocationId = destinationLocationId, Notes = notes });
    public Task<LotDto?> DispatchTransferAsync(Guid lotId, Guid destinationLocationId, string? notes) => PostAsync<DispatchTransferRequest, LotDto>("lots/transfer/dispatch", new DispatchTransferRequest { LotId = lotId, DestinationLocationId = destinationLocationId, Notes = notes });
    public Task<LotDto?> ReceiveTransferAsync(Guid lotId, Guid destinationLocationId, string? notes) => PostAsync<ReceiveTransferRequest, LotDto>("lots/transfer/receive", new ReceiveTransferRequest { LotId = lotId, DestinationLocationId = destinationLocationId, Notes = notes });

    // --- Movements ---
    public Task<List<MovementDto>?> GetMovementsByLotAsync(Guid lotId) => _http.GetFromJsonAsync<List<MovementDto>>($"lots/{lotId}/movements");

    // --- DEV ---
    public async Task ResetSeedAsync()
    {
        var response = await _http.PostAsync("dev/reset-seed", null);
        await EnsureSuccessAsync(response);
    }
}

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Body { get; }

    public ApiException(int statusCode, string message, string? body = null) : base(message)
    {
        StatusCode = statusCode;
        Body = body;
    }
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

public class CreateClientRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

public class UpdateClientRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

public class CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class UpdateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class CreateLocationRequest
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
}

public class UpdateLocationRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
}

public class LotDto
{
    public Guid LotId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? LocationId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateLotRequest
{
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class InboundLotRequest
{
    public Guid ClientId { get; set; }
    public Guid LocationId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class OutboundLotRequest
{
    public Guid LotId { get; set; }
    public string? Notes { get; set; }
}

public class MoveLotRequest
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

public class DispatchTransferRequest
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

public class ReceiveTransferRequest
{
    public Guid LotId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Notes { get; set; }
}

