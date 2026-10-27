using CasualDungeons.Domain.Entities;
using CasualDungeons.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class CombateConfiguration : IEntityTypeConfiguration<Combate>
{
    public void Configure(EntityTypeBuilder<Combate> builder)
    {
        builder.ToTable("Combates");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(EstadoCombate.EnPreparacion);

        builder.Property(c => c.Fecha).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.Property(c => c.RondaActual).HasDefaultValue(1);

        builder.HasIndex(c => c.CampaniaId);

        builder.HasOne(c => c.Campania)
            .WithMany(camp => camp.Combates)
            .HasForeignKey(c => c.CampaniaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
