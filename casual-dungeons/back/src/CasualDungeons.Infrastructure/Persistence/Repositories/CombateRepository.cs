using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class CombateRepository : EfRepository<Combate>, ICombateRepository
{
    public CombateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Combate?> GetByIdWithParticipantesAsync(int combateId, CancellationToken cancellationToken = default)
    {
        return await Context.Combates
            .Include(c => c.Participantes)
                .ThenInclude(p => p.Personaje)
            .Include(c => c.Participantes)
                .ThenInclude(p => p.NPC)
            .Include(c => c.Participantes)
                .ThenInclude(p => p.MonstruoCampania)
            .FirstOrDefaultAsync(c => c.Id == combateId, cancellationToken);
    }

    public async Task<List<Combate>> GetByCampaniaAsync(int campaniaId, CancellationToken cancellationToken = default)
    {
        return await Context.Combates
            .Include(c => c.Participantes)
                .ThenInclude(p => p.Personaje)
            .Include(c => c.Participantes)
                .ThenInclude(p => p.NPC)
            .Include(c => c.Participantes)
                .ThenInclude(p => p.MonstruoCampania)
            .Where(c => c.CampaniaId == campaniaId)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync(cancellationToken);
    }
}
