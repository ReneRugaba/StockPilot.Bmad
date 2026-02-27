using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Bmad.Domain.Locations;
using StockPilot.Bmad.Domain.Warehouses;

namespace StockPilot.Bmad.Infrastructure.Locations;

public class LocationEntityTypeConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(l => l.LocationId);

        builder.Property(l => l.WarehouseId)
            .IsRequired();

        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Label)
            .HasMaxLength(200);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(l => l.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

