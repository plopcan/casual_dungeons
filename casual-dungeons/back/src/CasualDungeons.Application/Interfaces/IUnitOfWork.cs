namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Coordina el guardado de cambios entre repositorios dentro de
/// una misma transacción implícita de EF Core (DbContext.SaveChanges).
/// </summary>
public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    ICampaniaRepository Campanias { get; }
    IPersonajeRepository Personajes { get; }
    INPCRepository NPCs { get; }
    IMonstruoCampaniaRepository MonstruosCampania { get; }
    ICombateRepository Combates { get; }
    IRolRepository Roles { get; }
    IRepository<Domain.Entities.ConfiguracionUsuario> ConfiguracionesUsuario { get; }
    IRepository<Domain.Entities.Hechizo> Hechizos { get; }
    IMonstruoCatalogoRepository MonstruosCatalogo { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
