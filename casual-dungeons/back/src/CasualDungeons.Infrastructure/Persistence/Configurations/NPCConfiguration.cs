using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class NPCConfiguration : IEntityTypeConfiguration<NPC>
{
    public void Configure(EntityTypeBuilder<NPC> builder)
    {
        builder.ToTable("NPCs", tb =>
        {
            tb.HasCheckConstraint("CK_NPCs_Atributos",
                "`Fuerza` BETWEEN 1 AND 30 AND `Destreza` BETWEEN 1 AND 30 AND " +
                "`Constitucion` BETWEEN 1 AND 30 AND `Inteligencia` BETWEEN 1 AND 30 AND " +
                "`Sabiduria` BETWEEN 1 AND 30 AND `Carisma` BETWEEN 1 AND 30");
            tb.HasCheckConstraint("CK_NPCs_PVNoNegativos", "`PuntosVida` >= 0 AND `PuntosVidaMaximos` >= 1");
        });

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(n => n.ImagenUrl).HasMaxLength(500);
        builder.Property(n => n.Notas).HasColumnType("longtext");

        builder.Property(n => n.Nivel).HasDefaultValue(1);
        builder.Property(n => n.PuntosVida).HasDefaultValue(1);
        builder.Property(n => n.PuntosVidaMaximos).HasDefaultValue(1);
        builder.Property(n => n.ClaseArmadura).HasDefaultValue(10);
        builder.Property(n => n.Fuerza).HasDefaultValue(10);
        builder.Property(n => n.Destreza).HasDefaultValue(10);
        builder.Property(n => n.Constitucion).HasDefaultValue(10);
        builder.Property(n => n.Inteligencia).HasDefaultValue(10);
        builder.Property(n => n.Sabiduria).HasDefaultValue(10);
        builder.Property(n => n.Carisma).HasDefaultValue(10);
        builder.Property(n => n.FechaCreacion).HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.Ignore(n => n.ModificadorFuerza);
        builder.Ignore(n => n.ModificadorDestreza);
        builder.Ignore(n => n.ModificadorConstitucion);
        builder.Ignore(n => n.ModificadorInteligencia);
        builder.Ignore(n => n.ModificadorSabiduria);
        builder.Ignore(n => n.ModificadorCarisma);

        builder.HasIndex(n => n.CampaniaId);

        builder.HasOne(n => n.Campania)
            .WithMany(c => c.NPCs)
            .HasForeignKey(n => n.CampaniaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
