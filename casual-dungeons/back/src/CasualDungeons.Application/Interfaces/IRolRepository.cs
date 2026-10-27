using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface IRolRepository : IRepository<Rol>
{
    Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default);
}
