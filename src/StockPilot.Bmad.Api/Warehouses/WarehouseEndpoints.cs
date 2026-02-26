using Microsoft.AspNetCore.Mvc;
using StockPilot.Bmad.Application.Warehouses;

namespace StockPilot.Bmad.Api.Warehouses;

public static class WarehouseEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/warehouses");

        group.MapPost("/", async ([FromBody] CreateWarehouseRequestDto dto, WarehouseService service, CancellationToken ct) =>
            {
                try
                {
                    var request = new CreateWarehouseRequest
                    {
                        Name = dto.Name,
                        Address = dto.Address
                    };

                    var result = await service.CreateWarehouseAsync(request, ct);
                    return Results.Created($"/warehouses/{result.WarehouseId}", result);
                }
                catch (WarehouseValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CreateWarehouse");

        group.MapGet("/", async (WarehouseService service, CancellationToken ct) =>
            {
                var result = await service.GetWarehousesAsync(ct);
                return Results.Ok(result);
            })
            .WithName("GetWarehouses");

        group.MapGet("/{warehouseId:guid}", async (Guid warehouseId, WarehouseService service, CancellationToken ct) =>
            {
                var result = await service.GetWarehouseByIdAsync(warehouseId, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetWarehouseById");

        return app;
    }
}

