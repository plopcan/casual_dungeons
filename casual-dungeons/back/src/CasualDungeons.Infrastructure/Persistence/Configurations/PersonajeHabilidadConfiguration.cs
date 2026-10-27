using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class PersonajeHabilidadConfiguration : IEntityTypeConfiguration<PersonajeHabilidad>
{
    public void Configure(EntityTypeBuilder<PersonajeHabilidad> builder)
    {
        builder.ToTable("PersonajeHabilidades");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.NombreHabilidad).HasMaxLength(50).IsRequired();
        builder.Property(h => h.AtributoBase).HasMaxLength(20).IsRequired();

        builder.HasIndex(h => new { h.PersonajeId, h.NombreHabilidad }).IsUnique();

        builder.HasOne(h => h.Personaje)
            .WithMany(p => p.Habilidades)
            .HasForeignKey(h => h.PersonajeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
