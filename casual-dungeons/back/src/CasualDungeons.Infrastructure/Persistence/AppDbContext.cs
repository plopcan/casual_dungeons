using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence;

/// <summary>
/// Contexto de base de datos principal. Todas las entidades del
/// dominio se exponen como DbSet y se mapean mediante clases de
/// configuración independientes (IEntityTypeConfiguration) en
/// Persistence/Configurations, manteniendo el contexto limpio.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ConfiguracionUsuario> ConfiguracionesUsuario => Set<ConfiguracionUsuario>();
    public DbSet<Campania> Campanias => Set<Campania>();
    public DbSet<ParticipanteCampania> ParticipantesCampania => Set<ParticipanteCampania>();
    public DbSet<Personaje> Personajes => Set<Personaje>();
    public DbSet<PersonajeHabilidad> PersonajeHabilidades => Set<PersonajeHabilidad>();
    public DbSet<Hechizo> Hechizos => Set<Hechizo>();
    public DbSet<PersonajeHechizo> PersonajeHechizos => Set<PersonajeHechizo>();
    public DbSet<PersonajeEspacioConjuro> PersonajeEspaciosConjuro => Set<PersonajeEspacioConjuro>();
    public DbSet<NPC> NPCs => Set<NPC>();
    public DbSet<MonstruoCampania> MonstruosCampania => Set<MonstruoCampania>();
    public DbSet<Combate> Combates => Set<Combate>();
    public DbSet<ParticipanteCombate> ParticipantesCombate => Set<ParticipanteCombate>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<InventarioItem> Inventario => Set<InventarioItem>();
    public DbSet<MonstruoCatalogo> MonstruosCatalogo => Set<MonstruoCatalogo>();
    public DbSet<MonstruoCatalogoDetalle> MonstruosCatalogoDetalles => Set<MonstruoCatalogoDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dnd");

        // Aplica todas las clases IEntityTypeConfiguration<T> del
        // ensamblado actual (una por entidad, en Configurations/).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
