using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Monstruos;
using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Services;

/// <summary>
/// Gestión de monstruos de campaña (bloques de estadísticas de
/// combate, sin ficha de rol) por parte del Dungeon Master. Solo el
/// DM propietario de la campaña puede crear, editar, eliminar o
/// consultar sus monstruos.
/// </summary>
public class MonstruoCampaniaService : IMonstruoCampaniaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignNotifier _campaignNotifier;

    public MonstruoCampaniaService(IUnitOfWork unitOfWork, ICampaignNotifier campaignNotifier)
    {
        _unitOfWork = unitOfWork;
        _campaignNotifier = campaignNotifier;
    }

    public async Task<Result<List<MonstruoCampaniaDto>>> GetByCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<List<MonstruoCampaniaDto>>.Fallo("Solo el Dungeon Master de la campaña puede ver sus monstruos.", ResultErrorTipo.SinPermiso);

        var monstruos = await _unitOfWork.MonstruosCampania.GetByCampaniaAsync(campaniaId, cancellationToken);
        return Result<List<MonstruoCampaniaDto>>.Exito(monstruos.Select(MapearADto).ToList());
    }

    public async Task<Result<MonstruoCampaniaDto>> GetByIdAsync(int monstruoCampaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var monstruo = await _unitOfWork.MonstruosCampania.GetByIdAsync(monstruoCampaniaId, cancellationToken);
        if (monstruo is null)
            return Result<MonstruoCampaniaDto>.Fallo("Monstruo no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(monstruo.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<MonstruoCampaniaDto>.Fallo("No tienes permiso para ver este monstruo.", ResultErrorTipo.SinPermiso);

        return Result<MonstruoCampaniaDto>.Exito(MapearADto(monstruo));
    }

    public async Task<Result<MonstruoCampaniaDto>> CrearAsync(int campaniaId, int dungeonMasterId, CrearMonstruoCampaniaDto dto, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<MonstruoCampaniaDto>.Fallo("Solo el Dungeon Master de la campaña puede crear monstruos.", ResultErrorTipo.SinPermiso);

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<MonstruoCampaniaDto>.Fallo("El nombre del monstruo es obligatorio.", ResultErrorTipo.Validacion);

        var validacion = ValidarAtributos(dto.Fuerza, dto.Destreza, dto.Constitucion, dto.Inteligencia, dto.Sabiduria, dto.Carisma);
        if (!validacion.EsExitoso)
            return Result<MonstruoCampaniaDto>.Fallo(validacion.Error!, validacion.TipoError!.Value);

        var monstruo = new MonstruoCampania
        {
            CampaniaId = campaniaId,
            Nombre = dto.Nombre.Trim(),
            ImagenUrl = dto.ImagenUrl,
            Tipo = dto.Tipo,
            Tamano = dto.Tamano,
            Alineamiento = dto.Alineamiento,
            ClaseArmadura = dto.ClaseArmadura,
            DescripcionArmadura = dto.DescripcionArmadura,
            PuntosVidaMaximos = dto.PuntosVidaMaximos,
            PuntosVidaActuales = dto.PuntosVidaActuales,
            DadoGolpe = dto.DadoGolpe,
            Velocidad = dto.Velocidad,
            Fuerza = dto.Fuerza,
            Destreza = dto.Destreza,
            Constitucion = dto.Constitucion,
            Inteligencia = dto.Inteligencia,
            Sabiduria = dto.Sabiduria,
            Carisma = dto.Carisma,
            Salvaciones = dto.Salvaciones,
            Habilidades = dto.Habilidades,
            ResistenciasDano = dto.ResistenciasDano,
            InmunidadesDano = dto.InmunidadesDano,
            InmunidadesEstado = dto.InmunidadesEstado,
            Sentidos = dto.Sentidos,
            Idiomas = dto.Idiomas,
            Desafio = dto.Desafio,
            PuntosExperiencia = dto.PuntosExperiencia,
            RasgosAcciones = dto.RasgosAcciones,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.MonstruosCampania.AddAsync(monstruo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MonstruoCampaniaDto>.Exito(MapearADto(monstruo));
    }

    public async Task<Result<MonstruoCampaniaDto>> CrearDesdeCatalogoAsync(int campaniaId, int dungeonMasterId, CrearMonstruoCampaniaDesdeCatalogoDto dto, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<MonstruoCampaniaDto>.Fallo("Solo el Dungeon Master de la campaña puede crear monstruos.", ResultErrorTipo.SinPermiso);

        var origen = await _unitOfWork.MonstruosCatalogo.GetConDetallesAsync(dto.MonstruoCatalogoId, cancellationToken);
        if (origen is null)
            return Result<MonstruoCampaniaDto>.Fallo("Monstruo no encontrado en el catálogo.", ResultErrorTipo.NoEncontrado);

        var monstruo = new MonstruoCampania
        {
            CampaniaId = campaniaId,
            Nombre = string.IsNullOrWhiteSpace(dto.NombrePersonalizado) ? origen.Nombre : dto.NombrePersonalizado.Trim(),
            ImagenUrl = null,
            Tipo = origen.Tipo,
            Tamano = origen.Tamano,
            Alineamiento = origen.Alineamiento,
            ClaseArmadura = origen.ClaseArmadura,
            DescripcionArmadura = origen.DescripcionArmadura,
            PuntosVidaMaximos = origen.PuntosVidaPromedio,
            PuntosVidaActuales = origen.PuntosVidaPromedio,
            DadoGolpe = origen.DadoGolpe,
            Velocidad = origen.Velocidad,
            Fuerza = origen.Fuerza,
            Destreza = origen.Destreza,
            Constitucion = origen.Constitucion,
            Inteligencia = origen.Inteligencia,
            Sabiduria = origen.Sabiduria,
            Carisma = origen.Carisma,
            Salvaciones = origen.Salvaciones,
            Habilidades = origen.Habilidades,
            ResistenciasDano = origen.ResistenciasDano,
            InmunidadesDano = origen.InmunidadesDano,
            InmunidadesEstado = origen.InmunidadesEstado,
            Sentidos = origen.Sentidos,
            Idiomas = origen.Idiomas,
            Desafio = origen.Desafio,
            PuntosExperiencia = origen.PuntosExperiencia,
            RasgosAcciones = FormatearRasgosAcciones(origen),
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.MonstruosCampania.AddAsync(monstruo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MonstruoCampaniaDto>.Exito(MapearADto(monstruo));
    }

    /// <summary>
    /// El catálogo modela Rasgos/Acciones/Acciones legendarias/
    /// Reacciones como filas estructuradas (MonstruoCatalogoDetalles);
    /// MonstruoCampania los guarda como un único bloque de texto, tal
    /// como pidió el DM, así que se vuelcan aquí en párrafos legibles.
    /// </summary>
    private static string? FormatearRasgosAcciones(MonstruoCatalogo monstruo)
    {
        if (monstruo.Detalles.Count == 0) return null;

        var lineas = new List<string>();
        foreach (var grupo in monstruo.Detalles.GroupBy(d => d.Tipo).OrderBy(g => (int)g.Key))
        {
            var tituloGrupo = grupo.Key switch
            {
                Domain.Enums.TipoDetalleMonstruo.Rasgo => "RASGOS",
                Domain.Enums.TipoDetalleMonstruo.Accion => "ACCIONES",
                Domain.Enums.TipoDetalleMonstruo.AccionLegendaria => "ACCIONES LEGENDARIAS",
                Domain.Enums.TipoDetalleMonstruo.Reaccion => "REACCIONES",
                _ => grupo.Key.ToString().ToUpperInvariant()
            };
            lineas.Add($"— {tituloGrupo} —");
            foreach (var item in grupo.OrderBy(d => d.Orden))
            {
                lineas.Add($"{item.Nombre}. {item.Descripcion}");
            }
            lineas.Add("");
        }

        return string.Join("\n", lineas).TrimEnd();
    }

    public async Task<Result<MonstruoCampaniaDto>> EditarAsync(int monstruoCampaniaId, int dungeonMasterId, EditarMonstruoCampaniaDto dto, CancellationToken cancellationToken = default)
    {
        var monstruo = await _unitOfWork.MonstruosCampania.GetByIdAsync(monstruoCampaniaId, cancellationToken);
        if (monstruo is null)
            return Result<MonstruoCampaniaDto>.Fallo("Monstruo no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(monstruo.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<MonstruoCampaniaDto>.Fallo("No tienes permiso para editar este monstruo.", ResultErrorTipo.SinPermiso);

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<MonstruoCampaniaDto>.Fallo("El nombre del monstruo es obligatorio.", ResultErrorTipo.Validacion);

        var validacion = ValidarAtributos(dto.Fuerza, dto.Destreza, dto.Constitucion, dto.Inteligencia, dto.Sabiduria, dto.Carisma);
        if (!validacion.EsExitoso)
            return Result<MonstruoCampaniaDto>.Fallo(validacion.Error!, validacion.TipoError!.Value);

        monstruo.Nombre = dto.Nombre.Trim();
        monstruo.ImagenUrl = dto.ImagenUrl;
        monstruo.Tipo = dto.Tipo;
        monstruo.Tamano = dto.Tamano;
        monstruo.Alineamiento = dto.Alineamiento;
        monstruo.ClaseArmadura = dto.ClaseArmadura;
        monstruo.DescripcionArmadura = dto.DescripcionArmadura;
        monstruo.PuntosVidaMaximos = dto.PuntosVidaMaximos;
        monstruo.PuntosVidaActuales = dto.PuntosVidaActuales;
        monstruo.DadoGolpe = dto.DadoGolpe;
        monstruo.Velocidad = dto.Velocidad;
        monstruo.Fuerza = dto.Fuerza;
        monstruo.Destreza = dto.Destreza;
        monstruo.Constitucion = dto.Constitucion;
        monstruo.Inteligencia = dto.Inteligencia;
        monstruo.Sabiduria = dto.Sabiduria;
        monstruo.Carisma = dto.Carisma;
        monstruo.Salvaciones = dto.Salvaciones;
        monstruo.Habilidades = dto.Habilidades;
        monstruo.ResistenciasDano = dto.ResistenciasDano;
        monstruo.InmunidadesDano = dto.InmunidadesDano;
        monstruo.InmunidadesEstado = dto.InmunidadesEstado;
        monstruo.Sentidos = dto.Sentidos;
        monstruo.Idiomas = dto.Idiomas;
        monstruo.Desafio = dto.Desafio;
        monstruo.PuntosExperiencia = dto.PuntosExperiencia;
        monstruo.RasgosAcciones = dto.RasgosAcciones;

        _unitOfWork.MonstruosCampania.Update(monstruo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarMonstruoCampaniaActualizadoAsync(monstruo.CampaniaId, monstruo.Id, cancellationToken);

        return Result<MonstruoCampaniaDto>.Exito(MapearADto(monstruo));
    }

    public async Task<Result> EliminarAsync(int monstruoCampaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var monstruo = await _unitOfWork.MonstruosCampania.GetByIdAsync(monstruoCampaniaId, cancellationToken);
        if (monstruo is null)
            return Result.Fallo("Monstruo no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(monstruo.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result.Fallo("No tienes permiso para eliminar este monstruo.", ResultErrorTipo.SinPermiso);

        // La FK MonstruosCampania->ParticipantesCombate es Restrict: si
        // este monstruo participó alguna vez en un combate, aunque ya
        // haya terminado, esa fila sigue referenciándolo y bloquea el
        // borrado si no se limpia antes (mismo caso que NPC).
        await _unitOfWork.MonstruosCampania.EliminarParticipacionesEnCombateAsync(monstruoCampaniaId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _unitOfWork.MonstruosCampania.Remove(monstruo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    private static Result ValidarAtributos(int fue, int des, int con, int inte, int sab, int car)
    {
        int[] atributos = { fue, des, con, inte, sab, car };
        if (atributos.Any(a => a is < 1 or > 30))
            return Result.Fallo("Los atributos deben estar entre 1 y 30.", ResultErrorTipo.Validacion);

        return Result.Exito();
    }

    private static MonstruoCampaniaDto MapearADto(MonstruoCampania m) => new(
        m.Id,
        m.CampaniaId,
        m.Nombre,
        m.ImagenUrl,
        m.Tipo,
        m.Tamano,
        m.Alineamiento,
        m.ClaseArmadura,
        m.DescripcionArmadura,
        m.PuntosVidaMaximos,
        m.PuntosVidaActuales,
        m.DadoGolpe,
        m.Velocidad,
        m.Fuerza,
        m.Destreza,
        m.Constitucion,
        m.Inteligencia,
        m.Sabiduria,
        m.Carisma,
        m.ModificadorFuerza,
        m.ModificadorDestreza,
        m.ModificadorConstitucion,
        m.ModificadorInteligencia,
        m.ModificadorSabiduria,
        m.ModificadorCarisma,
        m.Salvaciones,
        m.Habilidades,
        m.ResistenciasDano,
        m.InmunidadesDano,
        m.InmunidadesEstado,
        m.Sentidos,
        m.Idiomas,
        m.Desafio,
        m.PuntosExperiencia,
        m.RasgosAcciones);
}
