using Microsoft.AspNetCore.Builder;
using StockPilot.Bmad.Infrastructure;
using StockPilot.Bmad.Infrastructure.Seeding;

namespace StockPilot.Bmad.Api.Dev;

public static class DevEndpoints
{
    public static IEndpointRouteBuilder MapDevEndpoints(this IEndpointRouteBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
            return app;

        var group = app.MapGroup("/dev");

        group.MapPost("/reset-seed", async (StockPilotDbContext context, CancellationToken ct) =>
        {
            try
            {
                // Reset database
                await context.Database.EnsureDeletedAsync(ct);
                await context.Database.EnsureCreatedAsync(ct);

                // Seed demo data
                var seeder = new DemoDataSeeder(context);
                await seeder.SeedAsync(ct);

                return Results.Ok(new { message = "Database reset and seeded successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ResetSeed");

        return app;
    }
}

