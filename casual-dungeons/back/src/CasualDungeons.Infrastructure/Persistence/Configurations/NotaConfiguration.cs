using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class NotaConfiguration : IEntityTypeConfiguration<Nota>
{
    public void Configure(EntityTypeBuilder<Nota> builder)
    {
        builder.ToTable("Notas");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Titulo).HasMaxLength(150);
        builder.Property(n => n.Contenido).HasColumnType("longtext");
        builder.Property(n => n.FechaCreacion).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.Property(n => n.FechaActualizacion).HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.HasIndex(n => n.PersonajeId);

        builder.HasOne(n => n.Personaje)
            .WithMany(p => p.Notas)
            .HasForeignKey(n => n.PersonajeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
