using CasualDungeons.Application.DTOs.Monstruos;
using CasualDungeons.Application.DTOs.NPCs;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "DungeonMaster")]
public class NPCsController : ApiControllerBase
{
    private readonly INPCService _npcService;

    public NPCsController(INPCService npcService)
    {
        _npcService = npcService;
    }

    [HttpGet("campanias/{campaniaId:int}/npcs")]
    public async Task<IActionResult> GetByCampania(int campaniaId, CancellationToken cancellationToken)
    {
        var resultado = await _npcService.GetByCampaniaAsync(campaniaId, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpGet("npcs/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await _npcService.GetByIdAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPost("campanias/{campaniaId:int}/npcs")]
    public async Task<IActionResult> Crear(int campaniaId, [FromBody] CrearNPCDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _npcService.CrearAsync(campaniaId, UsuarioActualId, dto, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetById), new { id = resultado.Valor?.Id });
    }

    /// <summary>
    /// Crea un NPC a partir de un monstruo del catálogo precreado
    /// (selector "Añadir desde catálogo" en la pestaña NPC).
    /// </summary>
    [HttpPost("campanias/{campaniaId:int}/npcs/desde-catalogo")]
    public async Task<IActionResult> CrearDesdeCatalogo(int campaniaId, [FromBody] CrearNpcDesdeCatalogoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _npcService.CrearDesdeCatalogoAsync(campaniaId, UsuarioActualId, dto, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetById), new { id = resultado.Valor?.Id });
    }

    [HttpPut("npcs/{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarNPCDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _npcService.EditarAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("npcs/{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _npcService.EliminarAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }
}
