using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class CampaniaRepository : EfRepository<Campania>, ICampaniaRepository
{
    public CampaniaRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<Campania?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Campanias
            .Include(c => c.DungeonMaster)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Campania>> GetByUsuarioParticipanteAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await Context.Campanias
            .Include(c => c.DungeonMaster)
            .Where(c => c.DungeonMasterId == usuarioId || c.Participantes.Any(p => p.UsuarioId == usuarioId))
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<Campania?> GetByIdWithParticipantesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Campanias
            .Include(c => c.DungeonMaster)
            .Include(c => c.Participantes)
                .ThenInclude(p => p.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> EsParticipanteAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        return await Context.ParticipantesCampania
            .AnyAsync(p => p.CampaniaId == campaniaId && p.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<bool> EsDungeonMasterAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        return await Context.Campanias
            .AnyAsync(c => c.Id == campaniaId && c.DungeonMasterId == usuarioId, cancellationToken);
    }

    public async Task AgregarParticipanteAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        var yaExiste = await Context.ParticipantesCampania
            .AnyAsync(p => p.CampaniaId == campaniaId && p.UsuarioId == usuarioId, cancellationToken);

        if (yaExiste)
            return;

        await Context.ParticipantesCampania.AddAsync(new ParticipanteCampania
        {
            CampaniaId = campaniaId,
            UsuarioId = usuarioId,
            FechaIngreso = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task EliminarParticipanteAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        var participante = await Context.ParticipantesCampania
            .FirstOrDefaultAsync(p => p.CampaniaId == campaniaId && p.UsuarioId == usuarioId, cancellationToken);

        if (participante is not null)
            Context.ParticipantesCampania.Remove(participante);
    }
}
