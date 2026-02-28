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

        group.MapPut("/{clientId:guid}", async (Guid clientId, [FromBody] UpdateClientRequestDto dto, ClientService service, CancellationToken ct) =>
            {
                try
                {
                    var request = new UpdateClientRequest
                    {
                        Name = dto.Name,
                        ContactEmail = dto.ContactEmail
                    };
                    var result = await service.UpdateAsync(clientId, request, ct);
                    return Results.Ok(result);
                }
                catch (ClientNotFoundException)
                {
                    return Results.NotFound();
                }
                catch (ClientValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("UpdateClient");

        group.MapDelete("/{clientId:guid}", async (Guid clientId, ClientService service, CancellationToken ct) =>
            {
                try
                {
                    await service.DeactivateAsync(clientId, ct);
                    return Results.NoContent();
                }
                catch (ClientNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("DeactivateClient");

        return app;
    }
}
