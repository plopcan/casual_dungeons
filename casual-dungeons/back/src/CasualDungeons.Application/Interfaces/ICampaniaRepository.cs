using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface ICampaniaRepository : IRepository<Campania>
{
    /// <summary>
    /// Campañas en las que un usuario participa, ya sea como
    /// jugador (ParticipanteCampania) o como Dungeon Master propietario.
    /// </summary>
    Task<List<Campania>> GetByUsuarioParticipanteAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task<Campania?> GetByIdWithParticipantesAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> EsParticipanteAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default);

    Task<bool> EsDungeonMasterAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Añade un jugador como participante de la campaña.</summary>
    Task AgregarParticipanteAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Quita a un jugador de la campaña.</summary>
    Task EliminarParticipanteAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default);
}
