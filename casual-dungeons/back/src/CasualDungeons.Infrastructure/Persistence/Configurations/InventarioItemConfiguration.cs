using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CasualDungeons.Infrastructure.Persistence.Configurations;

public class InventarioItemConfiguration : IEntityTypeConfiguration<InventarioItem>
{
    public void Configure(EntityTypeBuilder<InventarioItem> builder)
    {
        builder.ToTable("Inventario", tb =>
        {
            tb.HasCheckConstraint("CK_Inventario_Propietario",
                "(`PersonajeId` IS NOT NULL AND `NPCId` IS NULL) OR (`PersonajeId` IS NULL AND `NPCId` IS NOT NULL)");
            tb.HasCheckConstraint("CK_Inventario_Cantidad", "`Cantidad` >= 0");
        });

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(i => i.Descripcion).HasMaxLength(1000);
        builder.Property(i => i.Peso).HasColumnType("decimal(6,2)");
        builder.Property(i => i.Tipo).HasMaxLength(50);
        builder.Property(i => i.Cantidad).HasDefaultValue(1);
        builder.Property(i => i.Equipado).HasDefaultValue(false);

        builder.HasIndex(i => i.PersonajeId);
        builder.HasIndex(i => i.NPCId);

        // IMPORTANTE: Personaje y NPC cuelgan ambos de Campania con
        // Cascade (Campania->Personaje y Campania->NPC). Si ambas FK
        // hacia Inventario también fueran Cascade, SQL Server
        // rechazaría el esquema por múltiples rutas de cascada que
        // convergen en Inventario desde el mismo ancestro (Campania).
        // Se deja Cascade en Personaje (caso más frecuente) y Restrict
        // en NPC; el borrado de un NPC con inventario debe limpiar
        // sus ítems explícitamente desde el servicio de aplicación
        // antes de eliminarlo.
        builder.HasOne(i => i.Personaje)
            .WithMany(p => p.Inventario)
            .HasForeignKey(i => i.PersonajeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.NPC)
            .WithMany(n => n.Inventario)
            .HasForeignKey(i => i.NPCId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
