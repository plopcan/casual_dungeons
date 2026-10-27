using CasualDungeons.Application.DTOs.Monstruos;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "DungeonMaster")]
public class MonstruosCampaniaController : ApiControllerBase
{
    private readonly IMonstruoCampaniaService _monstruoCampaniaService;

    public MonstruosCampaniaController(IMonstruoCampaniaService monstruoCampaniaService)
    {
        _monstruoCampaniaService = monstruoCampaniaService;
    }

    [HttpGet("campanias/{campaniaId:int}/monstruos-campania")]
    public async Task<IActionResult> GetByCampania(int campaniaId, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCampaniaService.GetByCampaniaAsync(campaniaId, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpGet("monstruos-campania/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCampaniaService.GetByIdAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPost("campanias/{campaniaId:int}/monstruos-campania")]
    public async Task<IActionResult> Crear(int campaniaId, [FromBody] CrearMonstruoCampaniaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCampaniaService.CrearAsync(campaniaId, UsuarioActualId, dto, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetById), new { id = resultado.Valor?.Id });
    }

    /// <summary>
    /// Crea un monstruo de campaña a partir de un monstruo del
    /// catálogo precreado (selector "Añadir desde catálogo" en la
    /// pestaña Monstruos), copiando el stat block completo.
    /// </summary>
    [HttpPost("campanias/{campaniaId:int}/monstruos-campania/desde-catalogo")]
    public async Task<IActionResult> CrearDesdeCatalogo(int campaniaId, [FromBody] CrearMonstruoCampaniaDesdeCatalogoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCampaniaService.CrearDesdeCatalogoAsync(campaniaId, UsuarioActualId, dto, cancellationToken);
        return resultado.ToCreatedResult(nameof(GetById), new { id = resultado.Valor?.Id });
    }

    [HttpPut("monstruos-campania/{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarMonstruoCampaniaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCampaniaService.EditarAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("monstruos-campania/{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCampaniaService.EliminarAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }
}
