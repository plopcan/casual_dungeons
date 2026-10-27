using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class ParticipanteCombateConfiguration : IEntityTypeConfiguration<ParticipanteCombate>
{
    public void Configure(EntityTypeBuilder<ParticipanteCombate> builder)
    {
        builder.ToTable("ParticipantesCombate", tb => tb.HasCheckConstraint(
            "CK_ParticipantesCombate_Origen",
            "(CASE WHEN `PersonajeId` IS NOT NULL THEN 1 ELSE 0 END) + " +
            "(CASE WHEN `NPCId` IS NOT NULL THEN 1 ELSE 0 END) + " +
            "(CASE WHEN `MonstruoCampaniaId` IS NOT NULL THEN 1 ELSE 0 END) = 1"));

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => new { p.CombateId, p.OrdenIniciativa }).IsUnique();
        builder.HasIndex(p => p.CombateId);

        // Propiedad calculada en memoria (no persistida).
        builder.Ignore(p => p.NombreParticipante);

        builder.HasOne(p => p.Combate)
            .WithMany(c => c.Participantes)
            .HasForeignKey(p => p.CombateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict en Personaje/NPC/MonstruoCampania para evitar
        // múltiples rutas de cascada en SQL Server (ya hay cascada
        // vía Combate). El borrado de un Personaje/NPC/MonstruoCampania
        // con participaciones activas debe gestionarse explícitamente
        // en el servicio de aplicación correspondiente.
        builder.HasOne(p => p.Personaje)
            .WithMany()
            .HasForeignKey(p => p.PersonajeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.NPC)
            .WithMany()
            .HasForeignKey(p => p.NPCId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.MonstruoCampania)
            .WithMany()
            .HasForeignKey(p => p.MonstruoCampaniaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
