using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class PersonajeEspacioConjuroConfiguration : IEntityTypeConfiguration<PersonajeEspacioConjuro>
{
    public void Configure(EntityTypeBuilder<PersonajeEspacioConjuro> builder)
    {
        builder.ToTable("PersonajeEspaciosConjuro", tb =>
        {
            tb.HasCheckConstraint("CK_EspaciosConjuro_Nivel", "`NivelConjuro` BETWEEN 1 AND 9");
            tb.HasCheckConstraint("CK_EspaciosConjuro_Cantidades",
                "`EspaciosUsados` <= `EspaciosTotales` AND `EspaciosTotales` >= 0 AND `EspaciosUsados` >= 0");
        });

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.PersonajeId, e.NivelConjuro }).IsUnique();

        // Propiedad calculada en memoria, no se mapea a columna.
        builder.Ignore(e => e.EspaciosDisponibles);

        builder.HasOne(e => e.Personaje)
            .WithMany(p => p.EspaciosConjuro)
            .HasForeignKey(e => e.PersonajeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
