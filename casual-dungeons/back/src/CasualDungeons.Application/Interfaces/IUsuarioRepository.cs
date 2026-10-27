using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByIdWithConfiguracionAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Usuarios activos que tienen el rol indicado (por nombre, ej.
    /// "Jugador"). Usado para selectores de invitación a campañas.
    /// </summary>
    Task<List<Usuario>> GetByRolNombreAsync(string nombreRol, CancellationToken cancellationToken = default);
}
