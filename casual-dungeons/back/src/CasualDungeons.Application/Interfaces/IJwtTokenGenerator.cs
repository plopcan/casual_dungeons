using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Genera el token JWT de acceso para un usuario autenticado.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera el token y devuelve también su fecha de expiración,
    /// útil para que el frontend sepa cuándo refrescar/redirigir a login.
    /// </summary>
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}
