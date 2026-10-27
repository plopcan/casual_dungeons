using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasualDungeons.Infrastructure.Persistence.Repositories;

public class RolRepository : EfRepository<Rol>, IRolRepository
{
    public RolRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        return await Context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre, cancellationToken);
    }
}
