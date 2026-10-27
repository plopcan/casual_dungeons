namespace CasualDungeons.Application.DTOs.Usuarios;

public record UsuarioDto(
    int Id,
    string NombreUsuario,
    string Email,
    string Rol,
    DateTime FechaCreacion,
    bool Activo,
    DateTime? UltimoAcceso);

public record CrearUsuarioDto(
    string NombreUsuario,
    string Password,
    string Email,
    string Rol);

public record EditarUsuarioDto(
    string NombreUsuario,
    string Email,
    string Rol,
    bool Activo,
    string? NuevaPassword);

public record ConfiguracionUsuarioDto(bool TemaOscuro);

/// <summary>
/// Proyección mínima de un usuario, sin datos sensibles (email,
/// fecha de creación, etc.), pensada para selectores donde el DM
/// elige jugadores a invitar a una campaña.
/// </summary>
public record UsuarioBasicoDto(int Id, string NombreUsuario);
