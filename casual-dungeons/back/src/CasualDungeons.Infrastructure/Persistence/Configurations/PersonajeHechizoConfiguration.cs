using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class PersonajeHechizoConfiguration : IEntityTypeConfiguration<PersonajeHechizo>
{
    public void Configure(EntityTypeBuilder<PersonajeHechizo> builder)
    {
        builder.ToTable("PersonajeHechizos");

        builder.HasKey(ph => ph.Id);

        builder.HasIndex(ph => new { ph.PersonajeId, ph.HechizoId }).IsUnique();
        builder.HasIndex(ph => ph.HechizoId);

        builder.HasOne(ph => ph.Personaje)
            .WithMany(p => p.Hechizos)
            .HasForeignKey(ph => ph.PersonajeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ph => ph.Hechizo)
            .WithMany(h => h.PersonajeHechizos)
            .HasForeignKey(ph => ph.HechizoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
