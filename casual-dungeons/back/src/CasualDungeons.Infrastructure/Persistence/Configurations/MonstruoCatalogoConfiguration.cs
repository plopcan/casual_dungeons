using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class MonstruoCatalogoConfiguration : IEntityTypeConfiguration<MonstruoCatalogo>
{
    public void Configure(EntityTypeBuilder<MonstruoCatalogo> builder)
    {
        builder.ToTable("MonstruosCatalogo");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Tipo).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Tamano).HasMaxLength(20).IsRequired();
        builder.Property(m => m.Alineamiento).HasMaxLength(50);
        builder.Property(m => m.DescripcionArmadura).HasMaxLength(100);
        builder.Property(m => m.DadoGolpe).HasMaxLength(20).IsRequired();
        builder.Property(m => m.Velocidad).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Salvaciones).HasMaxLength(150);
        builder.Property(m => m.Habilidades).HasMaxLength(200);
        builder.Property(m => m.ResistenciasDano).HasMaxLength(150);
        builder.Property(m => m.InmunidadesDano).HasMaxLength(150);
        builder.Property(m => m.InmunidadesEstado).HasMaxLength(150);
        builder.Property(m => m.Sentidos).HasMaxLength(150);
        builder.Property(m => m.Idiomas).HasMaxLength(150);
        builder.Property(m => m.Desafio).HasMaxLength(10).IsRequired();
        builder.Property(m => m.Fuente).HasMaxLength(50).HasDefaultValue("SRD 5.1");
        builder.Property(m => m.FechaCreacion).HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.HasIndex(m => m.Nombre);
        builder.HasIndex(m => m.Desafio);

        builder.HasMany(m => m.Detalles)
            .WithOne(d => d.Monstruo)
            .HasForeignKey(d => d.MonstruoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
