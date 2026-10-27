using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class NPCRepository : EfRepository<NPC>, INPCRepository
{
    public NPCRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<NPC>> GetByCampaniaAsync(int campaniaId, CancellationToken cancellationToken = default)
    {
        return await Context.NPCs
            .Where(n => n.CampaniaId == campaniaId)
            .OrderBy(n => n.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<NPC?> GetByIdWithInventarioAsync(int npcId, CancellationToken cancellationToken = default)
    {
        return await Context.NPCs
            .Include(n => n.Inventario)
            .FirstOrDefaultAsync(n => n.Id == npcId, cancellationToken);
    }
}
