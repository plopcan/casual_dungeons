using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

/// <summary>
/// Controlador base con utilidades comunes: extracción del Id de
/// usuario autenticado desde los claims del JWT.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Id del usuario autenticado, extraído del claim NameIdentifier
    /// emitido por JwtTokenGenerator. Lanza si no hay usuario
    /// autenticado (no debería ocurrir en endpoints con [Authorize]).
    /// </summary>
    protected int UsuarioActualId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("No se encontró el Id de usuario en el token."));
}
