using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class ConfiguracionUsuarioConfiguration : IEntityTypeConfiguration<ConfiguracionUsuario>
{
    public void Configure(EntityTypeBuilder<ConfiguracionUsuario> builder)
    {
        builder.ToTable("ConfiguracionesUsuario");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TemaOscuro)
            .HasDefaultValue(true);

        builder.Property(c => c.FechaActualizacion)
            .HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.HasIndex(c => c.UsuarioId).IsUnique();
    }
}
