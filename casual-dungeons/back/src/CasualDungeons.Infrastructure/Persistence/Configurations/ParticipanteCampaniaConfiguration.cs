using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class ParticipanteCampaniaConfiguration : IEntityTypeConfiguration<ParticipanteCampania>
{
    public void Configure(EntityTypeBuilder<ParticipanteCampania> builder)
    {
        builder.ToTable("ParticipantesCampania");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FechaIngreso)
            .HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.HasIndex(p => new { p.CampaniaId, p.UsuarioId }).IsUnique();
        builder.HasIndex(p => p.UsuarioId);

        builder.HasOne(p => p.Campania)
            .WithMany(c => c.Participantes)
            .HasForeignKey(p => p.CampaniaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Participaciones)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
