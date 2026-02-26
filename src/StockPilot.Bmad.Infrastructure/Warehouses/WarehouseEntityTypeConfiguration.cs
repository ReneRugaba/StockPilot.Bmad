using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Infrastructure.Warehouses;

public class WarehouseEntityTypeConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.WarehouseId);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .IsRequired();
    }
}

