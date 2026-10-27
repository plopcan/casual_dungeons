using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

/// <summary>
/// Catálogo de monstruos precreados (contenido abierto SRD 5.1) que
/// el DM puede usar para añadir NPCs a una campaña sin teclear su
/// estadística a mano. Solo lectura: el catálogo no se edita desde
/// la API, se mantiene con los scripts de seed de la base de datos.
/// </summary>
[ApiController]
[Route("api/monstruos-catalogo")]
[Authorize(Roles = "DungeonMaster")]
public class MonstruosCatalogoController : ApiControllerBase
{
    private readonly IMonstruoCatalogoService _monstruoCatalogoService;

    public MonstruosCatalogoController(IMonstruoCatalogoService monstruoCatalogoService)
    {
        _monstruoCatalogoService = monstruoCatalogoService;
    }

    /// <summary>Listado resumido para el selector. ?buscar= filtra por nombre.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTodos([FromQuery] string? buscar, CancellationToken cancellationToken)
    {
        var monstruos = await _monstruoCatalogoService.GetTodosAsync(buscar, cancellationToken);
        return Ok(monstruos);
    }

    /// <summary>Stat block completo, para previsualizar antes de añadirlo.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFicha(int id, CancellationToken cancellationToken)
    {
        var resultado = await _monstruoCatalogoService.GetFichaAsync(id, cancellationToken);
        return resultado.ToActionResult();
    }
}
