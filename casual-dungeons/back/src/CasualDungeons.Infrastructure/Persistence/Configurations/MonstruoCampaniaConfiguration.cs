using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class MonstruoCampaniaConfiguration : IEntityTypeConfiguration<MonstruoCampania>
{
    public void Configure(EntityTypeBuilder<MonstruoCampania> builder)
    {
        builder.ToTable("MonstruosCampania", tb =>
        {
            tb.HasCheckConstraint("CK_MonstruosCampania_Atributos",
                "`Fuerza` BETWEEN 1 AND 30 AND `Destreza` BETWEEN 1 AND 30 AND " +
                "`Constitucion` BETWEEN 1 AND 30 AND `Inteligencia` BETWEEN 1 AND 30 AND " +
                "`Sabiduria` BETWEEN 1 AND 30 AND `Carisma` BETWEEN 1 AND 30");
            tb.HasCheckConstraint("CK_MonstruosCampania_PVNoNegativos", "`PuntosVidaActuales` >= 0 AND `PuntosVidaMaximos` >= 1");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(m => m.ImagenUrl).HasMaxLength(500);
        builder.Property(m => m.Tipo).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Tamano).HasMaxLength(30).IsRequired();
        builder.Property(m => m.Alineamiento).HasMaxLength(50);
        builder.Property(m => m.DescripcionArmadura).HasMaxLength(200);
        builder.Property(m => m.DadoGolpe).HasMaxLength(50);
        builder.Property(m => m.Velocidad).HasMaxLength(200);
        builder.Property(m => m.Salvaciones).HasMaxLength(300);
        builder.Property(m => m.Habilidades).HasMaxLength(300);
        builder.Property(m => m.ResistenciasDano).HasMaxLength(300);
        builder.Property(m => m.InmunidadesDano).HasMaxLength(300);
        builder.Property(m => m.InmunidadesEstado).HasMaxLength(300);
        builder.Property(m => m.Sentidos).HasMaxLength(300);
        builder.Property(m => m.Idiomas).HasMaxLength(300);
        builder.Property(m => m.Desafio).HasMaxLength(10);
        builder.Property(m => m.RasgosAcciones).HasColumnType("longtext");

        builder.Property(m => m.ClaseArmadura).HasDefaultValue(10);
        builder.Property(m => m.PuntosVidaMaximos).HasDefaultValue(1);
        builder.Property(m => m.PuntosVidaActuales).HasDefaultValue(1);
        builder.Property(m => m.Fuerza).HasDefaultValue(10);
        builder.Property(m => m.Destreza).HasDefaultValue(10);
        builder.Property(m => m.Constitucion).HasDefaultValue(10);
        builder.Property(m => m.Inteligencia).HasDefaultValue(10);
        builder.Property(m => m.Sabiduria).HasDefaultValue(10);
        builder.Property(m => m.Carisma).HasDefaultValue(10);
        builder.Property(m => m.FechaCreacion).HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.Ignore(m => m.ModificadorFuerza);
        builder.Ignore(m => m.ModificadorDestreza);
        builder.Ignore(m => m.ModificadorConstitucion);
        builder.Ignore(m => m.ModificadorInteligencia);
        builder.Ignore(m => m.ModificadorSabiduria);
        builder.Ignore(m => m.ModificadorCarisma);

        builder.HasIndex(m => m.CampaniaId);

        builder.HasOne(m => m.Campania)
            .WithMany(c => c.MonstruosCampania)
            .HasForeignKey(m => m.CampaniaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
