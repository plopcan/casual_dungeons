using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class PersonajeRepository : EfRepository<Personaje>, IPersonajeRepository
{
    public PersonajeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Personaje?> GetFichaCompletaAsync(int personajeId, CancellationToken cancellationToken = default)
    {
        return await Context.Personajes
            .Include(p => p.Usuario)
            .Include(p => p.Habilidades)
            .Include(p => p.Hechizos)
                .ThenInclude(ph => ph.Hechizo)
            .Include(p => p.EspaciosConjuro)
            .Include(p => p.Inventario)
            .Include(p => p.Notas)
            .FirstOrDefaultAsync(p => p.Id == personajeId, cancellationToken);
    }

    public async Task<List<Personaje>> GetTodosByCampaniaYUsuarioAsync(int campaniaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        return await Context.Personajes
            .Include(p => p.Usuario)
            .Where(p => p.CampaniaId == campaniaId && p.UsuarioId == usuarioId)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Personaje>> GetByCampaniaAsync(int campaniaId, bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        var query = Context.Personajes
            .Include(p => p.Usuario)
            .Where(p => p.CampaniaId == campaniaId);

        if (soloActivos)
            query = query.Where(p => p.Activo);

        return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
    }
}
