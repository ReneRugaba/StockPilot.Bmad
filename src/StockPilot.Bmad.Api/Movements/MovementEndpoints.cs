using StockPilot.Bmad.Application.Lots;
using StockPilot.Bmad.Application.Movements;

namespace StockPilot.Bmad.Api.Movements;

public static class MovementEndpoints
{
    public static IEndpointRouteBuilder MapMovementEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/lots/{lotId:guid}/movements", async (Guid lotId, MovementQueryService service, CancellationToken ct) =>
            {
                try
                {
                    var result = await service.GetByLotIdAsync(lotId, ct);
                    return Results.Ok(result);
                }
                catch (LotNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("GetMovementsByLot");

        app.MapGet("/movements/{movementId:guid}", async (Guid movementId, MovementQueryService service, CancellationToken ct) =>
            {
                try
                {
                    var result = await service.GetByIdAsync(movementId, ct);
                    return Results.Ok(result);
                }
                catch (MovementNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("GetMovementById");

        return app;
    }
}

