using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Combates;

namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Gestión de combates e iniciativa, exclusiva del Dungeon Master
/// de la campaña.
/// </summary>
public interface ICombateService
{
    Task<Result<List<CombateDto>>> GetByCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<CombateDto>> GetByIdAsync(int combateId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<CombateDto>> CrearAsync(CrearCombateDto dto, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<CombateDto>> AgregarParticipanteAsync(int combateId, int dungeonMasterId, AgregarParticipanteCombateDto dto, CancellationToken cancellationToken = default);

    Task<Result> EliminarParticipanteAsync(int combateId, int participanteCombateId, int dungeonMasterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste el nuevo orden tras un drag &amp; drop en el frontend.
    /// </summary>
    Task<Result<CombateDto>> ReordenarParticipantesAsync(int combateId, int dungeonMasterId, ReordenarParticipantesDto dto, CancellationToken cancellationToken = default);

    Task<Result> FinalizarAsync(int combateId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result> EliminarAsync(int combateId, int dungeonMasterId, CancellationToken cancellationToken = default);
}
