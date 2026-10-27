using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

/// <summary>
/// Catálogo general de hechizos de 5e (solo lectura). Cualquier
/// usuario autenticado puede consultarlo para elegir qué hechizos
/// añadir a su personaje.
/// </summary>
[ApiController]
[Route("api/hechizos")]
[Authorize]
public class HechizosController : ApiControllerBase
{
    private readonly IHechizoService _hechizoService;

    public HechizosController(IHechizoService hechizoService)
    {
        _hechizoService = hechizoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var hechizos = await _hechizoService.GetAllAsync(cancellationToken);
        return Ok(hechizos);
    }
}
