using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Monstruos;
using CasualDungeons.Application.Interfaces;

namespace CasualDungeons.Application.Services;

public class MonstruoCatalogoService : IMonstruoCatalogoService
{
    private readonly IUnitOfWork _unitOfWork;

    public MonstruoCatalogoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<MonstruoCatalogoResumenDto>> GetTodosAsync(string? buscar = null, CancellationToken cancellationToken = default)
    {
        var monstruos = await _unitOfWork.MonstruosCatalogo.GetAllAsync(cancellationToken);

        var consulta = monstruos.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            consulta = consulta.Where(m => m.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase));
        }

        return consulta
            .OrderBy(m => OrdenDesafio(m.Desafio))
            .ThenBy(m => m.Nombre)
            .Select(m => new MonstruoCatalogoResumenDto(m.Id, m.Nombre, m.Tipo, m.Tamano, m.Desafio, m.PuntosVidaPromedio, m.ClaseArmadura))
            .ToList();
    }

    public async Task<Result<MonstruoCatalogoFichaDto>> GetFichaAsync(int id, CancellationToken cancellationToken = default)
    {
        var monstruo = await _unitOfWork.MonstruosCatalogo.GetConDetallesAsync(id, cancellationToken);
        if (monstruo is null)
            return Result<MonstruoCatalogoFichaDto>.Fallo("Monstruo no encontrado en el catálogo.", ResultErrorTipo.NoEncontrado);

        var dto = new MonstruoCatalogoFichaDto(
            monstruo.Id,
            monstruo.Nombre,
            monstruo.Tipo,
            monstruo.Tamano,
            monstruo.Alineamiento,
            monstruo.ClaseArmadura,
            monstruo.DescripcionArmadura,
            monstruo.PuntosVidaPromedio,
            monstruo.DadoGolpe,
            monstruo.Velocidad,
            monstruo.Fuerza,
            monstruo.Destreza,
            monstruo.Constitucion,
            monstruo.Inteligencia,
            monstruo.Sabiduria,
            monstruo.Carisma,
            monstruo.Salvaciones,
            monstruo.Habilidades,
            monstruo.ResistenciasDano,
            monstruo.InmunidadesDano,
            monstruo.InmunidadesEstado,
            monstruo.Sentidos,
            monstruo.Idiomas,
            monstruo.Desafio,
            monstruo.PuntosExperiencia,
            monstruo.Fuente,
            monstruo.Detalles
                .OrderBy(d => (int)d.Tipo)
                .ThenBy(d => d.Orden)
                .Select(d => new MonstruoCatalogoDetalleItemDto(d.Tipo.ToString(), d.Nombre, d.Descripcion))
                .ToList());

        return Result<MonstruoCatalogoFichaDto>.Exito(dto);
    }

    /// <summary>
    /// Convierte el desafío (texto, admite fracciones como "1/8") a un
    /// valor numérico ordenable: 0.125, 0.25, 0.5, 1, 2, 3... Los
    /// valores no reconocibles se envían al final.
    /// </summary>
    private static double OrdenDesafio(string desafio)
    {
        if (desafio.Contains('/'))
        {
            var partes = desafio.Split('/');
            if (partes.Length == 2 && double.TryParse(partes[0], out var num) && double.TryParse(partes[1], out var den) && den != 0)
                return num / den;
        }

        return double.TryParse(desafio, out var valor) ? valor : double.MaxValue;
    }
}
