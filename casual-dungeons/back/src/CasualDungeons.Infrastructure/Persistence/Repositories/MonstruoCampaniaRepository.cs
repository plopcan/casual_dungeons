using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class MonstruoCampaniaRepository : EfRepository<MonstruoCampania>, IMonstruoCampaniaRepository
{
    public MonstruoCampaniaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<MonstruoCampania>> GetByCampaniaAsync(int campaniaId, CancellationToken cancellationToken = default)
    {
        return await Context.MonstruosCampania
            .Where(m => m.CampaniaId == campaniaId)
            .OrderBy(m => m.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task EliminarParticipacionesEnCombateAsync(int monstruoCampaniaId, CancellationToken cancellationToken = default)
    {
        var participaciones = await Context.ParticipantesCombate
            .Where(p => p.MonstruoCampaniaId == monstruoCampaniaId)
            .ToListAsync(cancellationToken);

        if (participaciones.Count > 0)
        {
            Context.ParticipantesCombate.RemoveRange(participaciones);
        }
    }
}
