using StockPilot.Bmad.Domain.Clients;
using StockPilot.Bmad.Domain.Warehouses;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Lots;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Infrastructure.Seeding;

public class DemoDataSeeder
{
    private readonly StockPilotDbContext _context;

    public DemoDataSeeder(StockPilotDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Clients
        var client1 = Client.Create("Acme Corp", "contact@acme.com", now);
        var client2 = Client.Create("TechStart Inc", "info@techstart.com", now);

        _context.Clients.Add(client1);
        _context.Clients.Add(client2);

        // Warehouses
        var warehouse1 = Warehouse.Create("Entrepôt Paris", "123 rue de la Paix, 75000 Paris", now);
        var warehouse2 = Warehouse.Create("Entrepôt Lyon", "456 avenue de la République, 69000 Lyon", now);

        _context.Warehouses.Add(warehouse1);
        _context.Warehouses.Add(warehouse2);

        // Locations pour Warehouse 1
        var loc1_1 = Location.Create(warehouse1.WarehouseId, "A1", "Étagère A1", now);
        var loc1_2 = Location.Create(warehouse1.WarehouseId, "A2", "Étagère A2", now);
        var loc1_3 = Location.Create(warehouse1.WarehouseId, "B1", "Zone B1", now);
        var loc1_4 = Location.Create(warehouse1.WarehouseId, "B2", "Zone B2", now);

        _context.Locations.Add(loc1_1);
        _context.Locations.Add(loc1_2);
        _context.Locations.Add(loc1_3);
        _context.Locations.Add(loc1_4);

        // Locations pour Warehouse 2
        var loc2_1 = Location.Create(warehouse2.WarehouseId, "C1", "Colonne C1", now);
        var loc2_2 = Location.Create(warehouse2.WarehouseId, "C2", "Colonne C2", now);
        var loc2_3 = Location.Create(warehouse2.WarehouseId, "D1", "Dock D1", now);

        _context.Locations.Add(loc2_1);
        _context.Locations.Add(loc2_2);
        _context.Locations.Add(loc2_3);

        // Lots
        var lot1 = Lot.CreateInbound(client1.ClientId, loc1_3.LocationId, "LOT-2024-001", "Matériaux de construction", now);
        var lot2 = Lot.CreateInbound(client2.ClientId, loc2_2.LocationId, "LOT-2024-002", "Pièces informatiques", now);
        var lot3 = Lot.CreateInbound(client1.ClientId, loc1_4.LocationId, "LOT-2024-003", "Équipements en transit", now);

        _context.Lots.Add(lot1);
        _context.Lots.Add(lot2);
        _context.Lots.Add(lot3);


        await _context.SaveChangesAsync(ct);
    }
}
