using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Campanias;

namespace CasualDungeons.Application.Interfaces;

public interface ICampaniaService
{
    Task<List<CampaniaResumenDto>> GetMisCampaniasAsync(int usuarioId, bool activas, CancellationToken cancellationToken = default);

    Task<Result<CampaniaDetalleDto>> GetDetalleAsync(int campaniaId, int usuarioSolicitanteId, CancellationToken cancellationToken = default);

    Task<Result<CampaniaDetalleDto>> CrearAsync(int dungeonMasterId, CrearCampaniaDto dto, CancellationToken cancellationToken = default);

    Task<Result<CampaniaDetalleDto>> EditarAsync(int campaniaId, int dungeonMasterId, EditarCampaniaDto dto, CancellationToken cancellationToken = default);

    Task<Result> EliminarAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);
}
