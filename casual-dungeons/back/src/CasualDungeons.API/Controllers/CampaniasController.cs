using CasualDungeons.Application.DTOs.Campanias;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api/campanias")]
[Authorize]
public class CampaniasController : ApiControllerBase
{
    private readonly ICampaniaService _campaniaService;

    public CampaniasController(ICampaniaService campaniaService)
    {
        _campaniaService = campaniaService;
    }

    /// <summary>
    /// Campañas del usuario autenticado (como jugador o como DM),
    /// filtradas por estado. Pantalla principal con pestañas
    /// "Activas" / "Finalizadas".
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMisCampanias([FromQuery] bool activas = true, CancellationToken cancellationToken = default)
    {
        var campanias = await _campaniaService.GetMisCampaniasAsync(UsuarioActualId, activas, cancellationToken);
        return Ok(campanias);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetalle(int id, CancellationToken cancellationToken)
    {
        var resultado = await _campaniaService.GetDetalleAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Crea una campaña. Solo Dungeon Master.</summary>
    [HttpPost]
    [Authorize(Roles = "DungeonMaster")]
    public async Task<IActionResult> Crear([FromBody] CrearCampaniaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _campaniaService.CrearAsync(UsuarioActualId, dto, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetDetalle), new { id = resultado.Valor?.Id });
    }

    /// <summary>Edita una campaña. Solo el DM propietario.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "DungeonMaster")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarCampaniaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _campaniaService.EditarAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Elimina una campaña. Solo el DM propietario.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "DungeonMaster")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _campaniaService.EliminarAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }
}
