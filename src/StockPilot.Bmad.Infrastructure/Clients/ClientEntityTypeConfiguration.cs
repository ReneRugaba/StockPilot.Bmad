using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Bmad.Domain.Clients;

namespace StockPilot.Bmad.Infrastructure.Clients;

public class ClientEntityTypeConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();
    }
}

