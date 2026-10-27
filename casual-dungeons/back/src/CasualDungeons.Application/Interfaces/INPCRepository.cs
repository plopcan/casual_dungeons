using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface INPCRepository : IRepository<NPC>
{
    Task<List<NPC>> GetByCampaniaAsync(int campaniaId, CancellationToken cancellationToken = default);

    Task<NPC?> GetByIdWithInventarioAsync(int npcId, CancellationToken cancellationToken = default);
}
