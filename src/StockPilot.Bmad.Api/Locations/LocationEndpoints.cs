using Microsoft.AspNetCore.Mvc;
using StockPilot.Bmad.Application.Locations;

namespace StockPilot.Bmad.Api.Locations;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/locations");

        group.MapPost("/", async ([FromBody] CreateLocationRequestDto dto, LocationService service, CancellationToken ct) =>
            {
                try
                {
                    var request = new CreateLocationRequest
                    {
                        WarehouseId = dto.WarehouseId,
                        Code = dto.Code,
                        Label = dto.Label
                    };

                    var result = await service.CreateLocationAsync(request, ct);
                    return Results.Created($"/locations/{result.LocationId}", result);
                }
                catch (LocationValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CreateLocation");

        group.MapGet("/", async (LocationService service, CancellationToken ct) =>
            {
                var result = await service.GetLocationsAsync(ct);
                return Results.Ok(result);
            })
            .WithName("GetLocations");

        group.MapGet("/{locationId:guid}", async (Guid locationId, LocationService service, CancellationToken ct) =>
            {
                var result = await service.GetLocationByIdAsync(locationId, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetLocationById");

        app.MapGet("/warehouses/{warehouseId:guid}/locations", async (Guid warehouseId, LocationService service, CancellationToken ct) =>
            {
                var result = await service.GetLocationsByWarehouseAsync(warehouseId, ct);
                return Results.Ok(result);
            })
            .WithName("GetLocationsByWarehouse");

        return app;
    }
}

