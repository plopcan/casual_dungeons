using CasualDungeons.Domain.Entities;
using CasualDungeons.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class MonstruoCatalogoDetalleConfiguration : IEntityTypeConfiguration<MonstruoCatalogoDetalle>
{
    public void Configure(EntityTypeBuilder<MonstruoCatalogoDetalle> builder)
    {
        builder.ToTable("MonstruosCatalogoDetalles");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Descripcion).HasColumnType("text").IsRequired();
        builder.Property(d => d.Orden).HasDefaultValue(0);

        builder.HasIndex(d => d.MonstruoId);
    }
}
