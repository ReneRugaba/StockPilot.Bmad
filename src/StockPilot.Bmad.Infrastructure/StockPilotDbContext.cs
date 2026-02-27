using Microsoft.EntityFrameworkCore;
using StockPilot.Bmad.Domain.Clients;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;
using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Infrastructure;

public class StockPilotDbContext : DbContext
{
    public StockPilotDbContext(DbContextOptions<StockPilotDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<Movement> Movements => Set<Movement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockPilotDbContext).Assembly);
    }
}
