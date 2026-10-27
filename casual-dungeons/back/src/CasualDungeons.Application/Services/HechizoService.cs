using CasualDungeons.Application.DTOs.Hechizos;
using CasualDungeons.Application.Interfaces;

namespace CasualDungeons.Application.Services;

public class HechizoService : IHechizoService
{
    private readonly IUnitOfWork _unitOfWork;

    public HechizoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<HechizoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var hechizos = await _unitOfWork.Hechizos.GetAllAsync(cancellationToken);

        return hechizos
            .OrderBy(h => h.Nivel)
            .ThenBy(h => h.Nombre)
            .Select(h => new HechizoDto(
                h.Id,
                h.Nombre,
                h.Nivel,
                h.Escuela,
                h.TiempoLanzamiento,
                h.Alcance,
                h.Componentes,
                h.Duracion,
                h.Descripcion))
            .ToList();
    }
}
