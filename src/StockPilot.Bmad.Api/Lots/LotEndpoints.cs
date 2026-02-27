using Microsoft.AspNetCore.Mvc;
using StockPilot.Bmad.Application.Lots;

namespace StockPilot.Bmad.Api.Lots;

public static class LotEndpoints
{
    public static IEndpointRouteBuilder MapLotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/lots");

        group.MapPost("/inbound", async ([FromBody] InboundLotRequestDto dto, InboundLotService service, CancellationToken ct) =>
            {
                try
                {
                    var request = new InboundLotRequest
                    {
                        ClientId = dto.ClientId,
                        LocationId = dto.LocationId,
                        Reference = dto.Reference,
                        Description = dto.Description
                    };

                    var result = await service.InboundAsync(request, ct);
                    return Results.Created($"/lots/{result.LotId}", result);
                }
                catch (InboundLotValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("InboundLot");

        group.MapGet("/", async (LotQueryService service, CancellationToken ct) =>
            {
                var result = await service.GetAllAsync(ct);
                return Results.Ok(result);
            })
            .WithName("GetLots");

        group.MapGet("/{lotId:guid}", async (Guid lotId, LotQueryService service, CancellationToken ct) =>
            {
                try
                {
                    var result = await service.GetByIdAsync(lotId, ct);
                    return Results.Ok(result);
                }
                catch (LotNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("GetLotById");

        app.MapGet("/clients/{clientId:guid}/lots", async (Guid clientId, LotQueryService service, CancellationToken ct) =>
            {
                var result = await service.GetByClientIdAsync(clientId, ct);
                return Results.Ok(result);
            })
            .WithName("GetLotsByClient");

        app.MapGet("/warehouses/{warehouseId:guid}/lots", async (Guid warehouseId, LotQueryService service, CancellationToken ct) =>
            {
                var result = await service.GetByWarehouseIdAsync(warehouseId, ct);
                return Results.Ok(result);
            })
            .WithName("GetLotsByWarehouse");

        return app;
    }
}
