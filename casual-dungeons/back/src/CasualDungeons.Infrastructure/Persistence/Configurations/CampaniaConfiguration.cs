using CasualDungeons.Domain.Entities;
using CasualDungeons.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class CampaniaConfiguration : IEntityTypeConfiguration<Campania>
{
    public void Configure(EntityTypeBuilder<Campania> builder)
    {
        builder.ToTable("Campanias", tb => tb.HasCheckConstraint(
            "CK_Campanias_Fechas",
            "`FechaFinalizacion` IS NULL OR `FechaFinalizacion` >= `FechaInicio`"));

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Descripcion)
            .HasMaxLength(2000);

        builder.Property(c => c.ImagenUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(EstadoCampania.Activa);

        builder.Property(c => c.FechaCreacion)
            .HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.HasIndex(c => c.DungeonMasterId);
        builder.HasIndex(c => c.Estado);

        builder.HasOne(c => c.DungeonMaster)
            .WithMany(u => u.CampaniasComoDM)
            .HasForeignKey(c => c.DungeonMasterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
