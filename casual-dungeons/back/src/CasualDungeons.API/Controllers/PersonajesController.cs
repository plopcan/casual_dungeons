using CasualDungeons.Application.DTOs.Personajes;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class PersonajesController : ApiControllerBase
{
    private readonly IPersonajeService _personajeService;

    public PersonajesController(IPersonajeService personajeService)
    {
        _personajeService = personajeService;
    }

    /// <summary>
    /// Personajes del jugador autenticado en una campaña (Vista de
    /// campaña para jugadores). Puede devolver más de uno: el
    /// frontend decide si redirige directo a la ficha (1 resultado)
    /// o muestra un selector (varios resultados).
    /// </summary>
    [HttpGet("campanias/{campaniaId:int}/mis-personajes")]
    public async Task<IActionResult> GetMisPersonajes(int campaniaId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.GetMisPersonajesEnCampaniaAsync(campaniaId, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Crea un personaje nuevo (nivel 1) para el jugador autenticado
    /// en una campaña donde participa. Botón "Crear personaje" en la
    /// vista de campaña para jugadores.
    /// </summary>
    [HttpPost("campanias/{campaniaId:int}/personajes")]
    public async Task<IActionResult> Crear(int campaniaId, [FromBody] CrearPersonajeDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.CrearAsync(campaniaId, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Ficha de un personaje por Id, en modo solo lectura si quien
    /// consulta es el DM de la campaña (pestaña "Jugadores" del DM).
    /// </summary>
    [HttpGet("personajes/{id:int}")]
    public async Task<IActionResult> GetFicha(int id, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.GetFichaAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Listado resumido de personajes de una campaña. Solo el DM.
    /// </summary>
    [HttpGet("campanias/{campaniaId:int}/personajes")]
    [Authorize(Roles = "DungeonMaster")]
    public async Task<IActionResult> GetResumenPorCampania(int campaniaId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.GetResumenPorCampaniaAsync(campaniaId, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Activa o desactiva uno de los propios personajes. Solo el
    /// propietario. Los inactivos dejan de verse en la pantalla del DM.
    /// </summary>
    [HttpPatch("personajes/{id:int}/activo")]
    public async Task<IActionResult> CambiarActivo(int id, [FromBody] CambiarActivoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.CambiarActivoAsync(id, UsuarioActualId, dto.Activo, cancellationToken);
        return resultado.ToActionResult();
    }

    /// <summary>Actualiza la ficha completa. Solo el jugador propietario.</summary>
    [HttpPut("personajes/{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPersonajeDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.ActualizarAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    // ---------------- Inventario ----------------

    [HttpPost("personajes/{id:int}/inventario")]
    public async Task<IActionResult> AgregarItemInventario(int id, [FromBody] GuardarInventarioItemDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.AgregarItemInventarioAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("personajes/{id:int}/inventario/{itemId:int}")]
    public async Task<IActionResult> EliminarItemInventario(int id, int itemId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.EliminarItemInventarioAsync(id, UsuarioActualId, itemId, cancellationToken);
        return resultado.ToActionResult();
    }

    // ---------------- Notas ----------------

    [HttpPost("personajes/{id:int}/notas")]
    public async Task<IActionResult> AgregarNota(int id, [FromBody] GuardarNotaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.AgregarNotaAsync(id, UsuarioActualId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPut("personajes/{id:int}/notas/{notaId:int}")]
    public async Task<IActionResult> ActualizarNota(int id, int notaId, [FromBody] GuardarNotaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.ActualizarNotaAsync(id, UsuarioActualId, notaId, dto, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("personajes/{id:int}/notas/{notaId:int}")]
    public async Task<IActionResult> EliminarNota(int id, int notaId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.EliminarNotaAsync(id, UsuarioActualId, notaId, cancellationToken);
        return resultado.ToActionResult();
    }

    // ---------------- Habilidades ----------------

    [HttpPut("personajes/{id:int}/habilidades/{habilidadId:int}")]
    public async Task<IActionResult> ActualizarHabilidad(int id, int habilidadId, [FromBody] ActualizarHabilidadDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.ActualizarHabilidadAsync(id, UsuarioActualId, habilidadId, dto.Competente, dto.Experticia, cancellationToken);
        return resultado.ToActionResult();
    }

    // ---------------- Hechizos ----------------

    [HttpPost("personajes/{id:int}/hechizos/{hechizoId:int}")]
    public async Task<IActionResult> AgregarHechizo(int id, int hechizoId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.AgregarHechizoAsync(id, UsuarioActualId, hechizoId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpDelete("personajes/{id:int}/hechizos/{personajeHechizoId:int}")]
    public async Task<IActionResult> QuitarHechizo(int id, int personajeHechizoId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.QuitarHechizoAsync(id, UsuarioActualId, personajeHechizoId, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPatch("personajes/{id:int}/hechizos/{personajeHechizoId:int}/preparado")]
    public async Task<IActionResult> TogglePreparado(int id, int personajeHechizoId, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.TogglePreparadoAsync(id, UsuarioActualId, personajeHechizoId, cancellationToken);
        return resultado.ToActionResult();
    }

    // ---------------- Espacios de conjuro ----------------
    // El jugador introduce libremente sus espacios totales por nivel
    // (1-9); no se calculan a partir de la clase.

    [HttpPut("personajes/{id:int}/espacios-conjuro/{nivelConjuro:int}")]
    public async Task<IActionResult> ConfigurarEspacioConjuro(int id, int nivelConjuro, [FromBody] ConfigurarEspacioConjuroDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.ConfigurarEspacioConjuroAsync(id, UsuarioActualId, nivelConjuro, dto.EspaciosTotales, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPatch("personajes/{id:int}/espacios-conjuro/{nivelConjuro:int}/gastar")]
    public async Task<IActionResult> GastarEspacioConjuro(int id, int nivelConjuro, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.GastarEspacioConjuroAsync(id, UsuarioActualId, nivelConjuro, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPatch("personajes/{id:int}/espacios-conjuro/{nivelConjuro:int}/recuperar")]
    public async Task<IActionResult> RecuperarEspacioConjuro(int id, int nivelConjuro, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.RecuperarEspacioConjuroAsync(id, UsuarioActualId, nivelConjuro, cancellationToken);
        return resultado.ToActionResult();
    }

    [HttpPost("personajes/{id:int}/espacios-conjuro/resetear")]
    public async Task<IActionResult> ResetearEspaciosConjuro(int id, CancellationToken cancellationToken)
    {
        var resultado = await _personajeService.ResetearEspaciosConjuroAsync(id, UsuarioActualId, cancellationToken);
        return resultado.ToActionResult();
    }
}
