using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class PersonajeConfiguration : IEntityTypeConfiguration<Personaje>
{
    public void Configure(EntityTypeBuilder<Personaje> builder)
    {
        builder.ToTable("Personajes", tb =>
        {
            tb.HasCheckConstraint("CK_Personajes_Nivel", "`Nivel` BETWEEN 1 AND 20");
            tb.HasCheckConstraint("CK_Personajes_Atributos",
                "`Fuerza` BETWEEN 1 AND 30 AND `Destreza` BETWEEN 1 AND 30 AND " +
                "`Constitucion` BETWEEN 1 AND 30 AND `Inteligencia` BETWEEN 1 AND 30 AND " +
                "`Sabiduria` BETWEEN 1 AND 30 AND `Carisma` BETWEEN 1 AND 30");
            tb.HasCheckConstraint("CK_Personajes_PVNoNegativos",
                "`PuntosVidaActuales` >= 0 AND `PuntosVidaMaximos` >= 1 AND `PuntosVidaTemporales` >= 0");
            tb.HasCheckConstraint("CK_Personajes_Experiencia", "`Experiencia` >= 0");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Raza).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Clase).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Subclase).HasMaxLength(50);
        builder.Property(p => p.Trasfondo).HasMaxLength(50);
        builder.Property(p => p.Alineamiento).HasMaxLength(30);
        builder.Property(p => p.ImagenUrl).HasMaxLength(500);
        builder.Property(p => p.DadoGolpe).HasMaxLength(10);
        builder.Property(p => p.NotasGenerales).HasColumnType("longtext");

        builder.Property(p => p.Nivel).HasDefaultValue(1);
        builder.Property(p => p.Experiencia).HasDefaultValue(0);
        builder.Property(p => p.PuntosVidaMaximos).HasDefaultValue(1);
        builder.Property(p => p.DadosGolpeRestantes).HasDefaultValue(1);
        builder.Property(p => p.ClaseArmadura).HasDefaultValue(10);
        builder.Property(p => p.Velocidad).HasDefaultValue(30);
        builder.Property(p => p.PercepcionPasiva).HasDefaultValue(10);
        builder.Property(p => p.Activo).HasDefaultValue(true);

        builder.Property(p => p.Fuerza).HasDefaultValue(10);
        builder.Property(p => p.Destreza).HasDefaultValue(10);
        builder.Property(p => p.Constitucion).HasDefaultValue(10);
        builder.Property(p => p.Inteligencia).HasDefaultValue(10);
        builder.Property(p => p.Sabiduria).HasDefaultValue(10);
        builder.Property(p => p.Carisma).HasDefaultValue(10);

        builder.Property(p => p.FechaCreacion).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.Property(p => p.FechaActualizacion).HasDefaultValueSql("UTC_TIMESTAMP()");

        // Propiedades calculadas en memoria: NO se mapean a columnas.
        builder.Ignore(p => p.ModificadorFuerza);
        builder.Ignore(p => p.ModificadorDestreza);
        builder.Ignore(p => p.ModificadorConstitucion);
        builder.Ignore(p => p.ModificadorInteligencia);
        builder.Ignore(p => p.ModificadorSabiduria);
        builder.Ignore(p => p.ModificadorCarisma);
        builder.Ignore(p => p.BonificadorCompetencia);

        // Antes existía un índice único aquí para forzar "1 personaje
        // por jugador por campaña". Regla de mesa habitual en D&D 5e,
        // pero no obligatoria: se permite que un jugador tenga varios
        // personajes en la misma campaña (ej. tras la muerte de uno,
        // o llevar varios PCs a la vez). Se mantiene como índice NO
        // único: sigue acelerando las consultas "personajes de este
        // usuario en esta campaña" sin bloquear la inserción de un
        // segundo personaje.
        builder.HasIndex(p => new { p.CampaniaId, p.UsuarioId });
        builder.HasIndex(p => p.CampaniaId);
        builder.HasIndex(p => p.UsuarioId);

        builder.HasOne(p => p.Campania)
            .WithMany(c => c.Personajes)
            .HasForeignKey(p => p.CampaniaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Personajes)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
