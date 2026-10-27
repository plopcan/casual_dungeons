using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class HechizoConfiguration : IEntityTypeConfiguration<Hechizo>
{
    public void Configure(EntityTypeBuilder<Hechizo> builder)
    {
        builder.ToTable("Hechizos", tb => tb.HasCheckConstraint(
            "CK_Hechizos_Nivel", "`Nivel` BETWEEN 0 AND 9"));

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Escuela).HasMaxLength(50);
        builder.Property(h => h.TiempoLanzamiento).HasMaxLength(50);
        builder.Property(h => h.Alcance).HasMaxLength(50);
        builder.Property(h => h.Componentes).HasMaxLength(200);
        builder.Property(h => h.Duracion).HasMaxLength(50);
        builder.Property(h => h.Descripcion).HasColumnType("longtext");

        builder.HasIndex(h => h.Nombre).IsUnique();
    }
}
