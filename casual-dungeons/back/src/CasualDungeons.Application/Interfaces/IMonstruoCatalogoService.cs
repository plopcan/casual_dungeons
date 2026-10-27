using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Monstruos;

namespace CasualDungeons.Application.Interfaces;

public interface IMonstruoCatalogoService
{
    /// <summary>Listado resumido para el selector, opcionalmente filtrado por nombre y/o desafío.</summary>
    Task<List<MonstruoCatalogoResumenDto>> GetTodosAsync(string? buscar = null, CancellationToken cancellationToken = default);

    Task<Result<MonstruoCatalogoFichaDto>> GetFichaAsync(int id, CancellationToken cancellationToken = default);
}
