using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface IPersonajeRepository : IRepository<Personaje>
{
    /// <summary>
    /// Ficha completa de un personaje con todas sus relaciones
    /// (habilidades, hechizos, espacios de conjuro, inventario, notas)
    /// para mostrar/editar la ficha completa.
    /// </summary>
    Task<Personaje?> GetFichaCompletaAsync(int personajeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Todos los personajes que un jugador tiene en una campaña
    /// concreta. Puede haber más de uno: no se fuerza la convención
    /// de mesa "1 personaje por jugador por campaña" a nivel de
    /// aplicación ni de base de datos.
    /// </summary>
    Task<List<Personaje>> GetTodosByCampaniaYUsuarioAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Todos los personajes de una campaña con datos resumidos
    /// (para la pestaña "Jugadores" del DM). Con soloActivos=true
    /// (usado por la vista del DM) se excluyen los personajes que su
    /// jugador haya desactivado.
    /// </summary>
    Task<List<Personaje>> GetByCampaniaAsync(int campaniaId, bool soloActivos = false, CancellationToken cancellationToken = default);
}
