using Microsoft.AspNetCore.Mvc;
using StockPilot.Bmad.Application.Clients;

namespace StockPilot.Bmad.Api.Clients;

public static class ClientEndpoints
{
    public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/clients");

        group.MapPost("/", async ([FromBody] CreateClientRequestDto dto, ClientService service, CancellationToken ct) =>
            {
                try
                {
                    var request = new CreateClientRequest
                    {
                        Name = dto.Name,
                        ContactEmail = dto.ContactEmail
                    };

                    var result = await service.CreateClientAsync(request, ct);
                    return Results.Created($"/clients/{result.ClientId}", result);
                }
                catch (ClientValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CreateClient");

        group.MapGet("/", async (ClientService service, CancellationToken ct) =>
            {
                var result = await service.GetClientsAsync(ct);
                return Results.Ok(result);
            })
            .WithName("GetClients");

        group.MapGet("/{clientId:guid}", async (Guid clientId, ClientService service, CancellationToken ct) =>
            {
                var result = await service.GetClientByIdAsync(clientId, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetClientById");

        return app;
    }
}
