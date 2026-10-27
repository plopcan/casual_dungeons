using CasualDungeons.Application.DTOs.Hechizos;

namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Consulta de solo lectura sobre el catálogo general de hechizos
/// (tabla dnd.Hechizos), usado por los jugadores para elegir qué
/// hechizos añadir a su personaje.
/// </summary>
public interface IHechizoService
{
    Task<List<HechizoDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
