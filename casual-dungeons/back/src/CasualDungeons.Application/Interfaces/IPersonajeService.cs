using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Personajes;

namespace CasualDungeons.Application.Interfaces;

public interface IPersonajeService
{
    /// <summary>
    /// Ficha completa del personaje. SoloLectura=true si quien
    /// consulta es el DM (ve la ficha pero no puede editarla salvo
    /// que el servicio de actualización lo permita explícitamente).
    /// </summary>
    Task<Result<PersonajeFichaDto>> GetFichaAsync(int personajeId, int usuarioSolicitanteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Personajes del propio jugador dentro de una campaña (pantalla
    /// principal de "Vista de campaña para jugadores"). Devuelve una
    /// lista porque un jugador puede tener más de un personaje en la
    /// misma campaña; si solo tiene uno, el frontend puede saltar
    /// directamente a su ficha sin mostrar un selector.
    /// </summary>
    Task<Result<List<PersonajeResumenDto>>> GetMisPersonajesEnCampaniaAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista resumida de personajes de una campaña (pestaña
    /// "Jugadores" de la vista del DM).
    /// </summary>
    Task<Result<List<PersonajeResumenDto>>> GetResumenPorCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un personaje de nivel 1 en una campaña donde el usuario
    /// participa como jugador. Los valores derivados (PV, CA,
    /// iniciativa, percepción pasiva) se calculan aplicando las
    /// reglas de D&D 5e a partir de la clase y los atributos.
    /// </summary>
    Task<Result<PersonajeFichaDto>> CrearAsync(int campaniaId, int usuarioId, CrearPersonajeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa o desactiva uno de los propios personajes. Solo el
    /// propietario puede hacerlo. Los personajes inactivos dejan de
    /// aparecer en la pantalla del DM y como opción en combate.
    /// </summary>
    Task<Result<PersonajeActivoDto>> CambiarActivoAsync(int personajeId, int usuarioId, bool activo, CancellationToken cancellationToken = default);

    Task<Result<PersonajeFichaDto>> ActualizarAsync(int personajeId, int usuarioId, ActualizarPersonajeDto dto, CancellationToken cancellationToken = default);

    Task<Result<InventarioItemDto>> AgregarItemInventarioAsync(int personajeId, int usuarioId, GuardarInventarioItemDto dto, CancellationToken cancellationToken = default);

    Task<Result> EliminarItemInventarioAsync(int personajeId, int usuarioId, int itemId, CancellationToken cancellationToken = default);

    Task<Result<NotaDto>> AgregarNotaAsync(int personajeId, int usuarioId, GuardarNotaDto dto, CancellationToken cancellationToken = default);

    Task<Result<NotaDto>> ActualizarNotaAsync(int personajeId, int usuarioId, int notaId, GuardarNotaDto dto, CancellationToken cancellationToken = default);

    Task<Result> EliminarNotaAsync(int personajeId, int usuarioId, int notaId, CancellationToken cancellationToken = default);

    // Habilidades (pestaña "Habilidades")
    Task<Result<PersonajeHabilidadDto>> ActualizarHabilidadAsync(int personajeId, int usuarioId, int habilidadId, bool competente, bool experticia, CancellationToken cancellationToken = default);

    // Hechizos (pestaña "Hechizos")
    Task<Result<PersonajeHechizoDto>> AgregarHechizoAsync(int personajeId, int usuarioId, int hechizoId, CancellationToken cancellationToken = default);
    Task<Result> QuitarHechizoAsync(int personajeId, int usuarioId, int personajeHechizoId, CancellationToken cancellationToken = default);
    Task<Result<PersonajeHechizoDto>> TogglePreparadoAsync(int personajeId, int usuarioId, int personajeHechizoId, CancellationToken cancellationToken = default);

    // Espacios de conjuro (pestaña "Hechizos"). Los totales por nivel
    // los introduce libremente el propio jugador (no se calculan a
    // partir de la clase, ya que Clase es texto libre y puede ser
    // una clase homebrew no contemplada por ninguna tabla oficial).
    Task<Result<PersonajeEspacioConjuroDto>> ConfigurarEspacioConjuroAsync(int personajeId, int usuarioId, int nivelConjuro, int espaciosTotales, CancellationToken cancellationToken = default);
    Task<Result<PersonajeEspacioConjuroDto>> GastarEspacioConjuroAsync(int personajeId, int usuarioId, int nivelConjuro, CancellationToken cancellationToken = default);
    Task<Result<PersonajeEspacioConjuroDto>> RecuperarEspacioConjuroAsync(int personajeId, int usuarioId, int nivelConjuro, CancellationToken cancellationToken = default);
    Task<Result<List<PersonajeEspacioConjuroDto>>> ResetearEspaciosConjuroAsync(int personajeId, int usuarioId, CancellationToken cancellationToken = default);
}
