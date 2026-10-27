using CasualDungeons.Application.DTOs.Auth;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Inicia sesión y devuelve el token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var resultado = await _authService.LoginAsync(request, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Cambia la contraseña del usuario autenticado.</summary>
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var resultado = await _authService.CambiarPasswordAsync(UsuarioActualId, request, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Cambia el nombre de usuario del usuario autenticado.</summary>
    [HttpPut("nombre-usuario")]
    [Authorize]
    public async Task<IActionResult> CambiarNombreUsuario([FromBody] CambiarNombreUsuarioRequestDto request, CancellationToken cancellationToken)
    {
        var resultado = await _authService.CambiarNombreUsuarioAsync(UsuarioActualId, request, cancellationToken);
        return resultado.ToActionResult();
    }
}
