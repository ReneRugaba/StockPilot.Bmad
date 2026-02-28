using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Bmad.Domain.Lots;

namespace StockPilot.Bmad.Infrastructure.Lots;

public class LotEntityTypeConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("Lots");

        builder.HasKey(l => l.LotId);

        builder.Property(l => l.ClientId)
            .IsRequired();

        builder.Property(l => l.LocationId)
            .IsRequired(false);

        builder.Property(l => l.Reference)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .IsRequired();
    }
}
