using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Bmad.Domain.Movements;

namespace StockPilot.Bmad.Infrastructure.Movements;

public class MovementEntityTypeConfiguration : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.ToTable("Movements");

        builder.HasKey(m => m.MovementId);

        builder.Property(m => m.LotId)
            .IsRequired();

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.FromLocationId);

        builder.Property(m => m.ToLocationId);

        builder.Property(m => m.OccurredAt)
            .IsRequired();

        builder.Property(m => m.PerformedBy)
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasMaxLength(500);
    }
}

