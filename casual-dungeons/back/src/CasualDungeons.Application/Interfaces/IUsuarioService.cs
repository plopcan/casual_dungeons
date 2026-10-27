using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Usuarios;

namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Operaciones de administración de usuarios, exclusivas del rol
/// Administrador.
/// </summary>
public interface IUsuarioService
{
    Task<List<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UsuarioDto>> CrearAsync(CrearUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<Result<UsuarioDto>> EditarAsync(int id, EditarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<Result> EliminarAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ConfiguracionUsuarioDto>> GetConfiguracionAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<Result<ConfiguracionUsuarioDto>> ActualizarConfiguracionAsync(int usuarioId, ConfiguracionUsuarioDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista mínima de jugadores activos, para que un Dungeon Master
    /// pueda seleccionarlos al crear o editar una campaña.
    /// </summary>
    Task<List<UsuarioBasicoDto>> GetJugadoresAsync(CancellationToken cancellationToken = default);
}
