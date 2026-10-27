using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class MonstruoCatalogoRepository : EfRepository<MonstruoCatalogo>, IMonstruoCatalogoRepository
{
    public MonstruoCatalogoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<MonstruoCatalogo?> GetConDetallesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.MonstruosCatalogo
            .Include(m => m.Detalles)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }
}
