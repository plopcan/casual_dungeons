using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface ICombateRepository : IRepository<Combate>
{
    /// <summary>
    /// Combate con todos sus participantes (incluyendo datos de
    /// Personaje o NPC asociados), ordenados por OrdenIniciativa.
    /// </summary>
    Task<Combate?> GetByIdWithParticipantesAsync(int combateId, CancellationToken cancellationToken = default);

    Task<List<Combate>> GetByCampaniaAsync(int campaniaId, CancellationToken cancellationToken = default);
}
