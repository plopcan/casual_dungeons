using CasualDungeons.Application.DTOs.Combates;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "DungeonMaster")]
public class CombatesController : ApiControllerBase
{
    private readonly ICombateService _combateService;

    public CombatesController(ICombateService combateService)
    {
        _combateService = combateService;
    }

    [HttpGet("campanias/{campaniaId:int}/combates")]
    public async Task<IActionResult> GetByCampania(int campaniaId, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.GetByCampaniaAsync(campaniaId, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpGet("combates/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.GetByIdAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPost("combates")]
    public async Task<IActionResult> Crear([FromBody] CrearCombateDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.CrearAsync(dto, UsuarioActualId, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetById), new { id = resultado.Valor?.Id });
    }

    [HttpPost("combates/{id:int}/participantes")]
    public async Task<IActionResult> AgregarParticipante(int id, [FromBody] AgregarParticipanteCombateDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.AgregarParticipanteAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("combates/{id:int}/participantes/{participanteId:int}")]
    public async Task<IActionResult> EliminarParticipante(int id, int participanteId, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.EliminarParticipanteAsync(id, participanteId, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Persiste el nuevo orden de iniciativa tras un drag &amp; drop
    /// en el frontend.
    /// </summary>
    [HttpPut("combates/{id:int}/orden")]
    public async Task<IActionResult> Reordenar(int id, [FromBody] ReordenarParticipantesDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.ReordenarParticipantesAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPost("combates/{id:int}/finalizar")]
    public async Task<IActionResult> Finalizar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.FinalizarAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("combates/{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _combateService.EliminarAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }
}
