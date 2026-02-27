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

        return app;
    }
}

