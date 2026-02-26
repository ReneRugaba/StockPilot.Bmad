using Microsoft.EntityFrameworkCore;
using StockPilot.Bmad.Domain.Clients;
using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Infrastructure;

public class StockPilotDbContext : DbContext
{
    public StockPilotDbContext(DbContextOptions<StockPilotDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockPilotDbContext).Assembly);
    }
}
