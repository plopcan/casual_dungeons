using CasualDungeons.Application.DTOs.Usuarios;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ApiControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>Lista todos los usuarios. Solo Administrador.</summary>
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.GetAllAsync(cancellationToken);
        return Ok(usuarios);
    }

    /// <summary>
    /// Lista mínima de jugadores activos (Id + nombre), para que un
    /// Dungeon Master pueda invitarlos a sus campañas.
    /// </summary>
    [HttpGet("jugadores")]
    [Authorize(Roles = "DungeonMaster,Administrador")]
    public async Task<IActionResult> GetJugadores(CancellationToken cancellationToken)
    {
        var jugadores = await _usuarioService.GetJugadoresAsync(cancellationToken);
        return Ok(jugadores);
    }

    /// <summary>Consulta un usuario por Id. Solo Administrador.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await _usuarioService.GetByIdAsync(id, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Crea un nuevo usuario. Solo Administrador.</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _usuarioService.CrearAsync(dto, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetById), new { id = resultado.Valor?.Id });
    }

    /// <summary>Edita un usuario existente. Solo Administrador.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _usuarioService.EditarAsync(id, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Desactiva un usuario (eliminación lógica). Solo Administrador.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _usuarioService.EliminarAsync(id, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Configuración personal del usuario autenticado.</summary>
    [HttpGet("me/configuracion")]
    public async Task<IActionResult> GetMiConfiguracion(CancellationToken cancellationToken)
    {
        var resultado = await _usuarioService.GetConfiguracionAsync(UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Actualiza la configuración personal (tema claro/oscuro).</summary>
    [HttpPut("me/configuracion")]
    public async Task<IActionResult> ActualizarMiConfiguracion([FromBody] ConfiguracionUsuarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _usuarioService.ActualizarConfiguracionAsync(UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }
}
