using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Monstruos;

namespace CasualDungeons.Application.Interfaces;

public interface IMonstruoCampaniaService
{
    Task<Result<List<MonstruoCampaniaDto>>> GetByCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<MonstruoCampaniaDto>> GetByIdAsync(int monstruoCampaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<MonstruoCampaniaDto>> CrearAsync(int campaniaId, int dungeonMasterId, CrearMonstruoCampaniaDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un monstruo de campaña copiando TODOS los campos de un
    /// monstruo del catálogo precreado (stat block completo).
    /// </summary>
    Task<Result<MonstruoCampaniaDto>> CrearDesdeCatalogoAsync(int campaniaId, int dungeonMasterId, CrearMonstruoCampaniaDesdeCatalogoDto dto, CancellationToken cancellationToken = default);

    Task<Result<MonstruoCampaniaDto>> EditarAsync(int monstruoCampaniaId, int dungeonMasterId, EditarMonstruoCampaniaDto dto, CancellationToken cancellationToken = default);

    Task<Result> EliminarAsync(int monstruoCampaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);
}
