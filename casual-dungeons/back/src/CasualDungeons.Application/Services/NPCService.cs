using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Monstruos;
using CasualDungeons.Application.DTOs.NPCs;
using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Services;

/// <summary>
/// Gestión de NPCs por parte del Dungeon Master. Solo el DM
/// propietario de la campaña puede crear, editar, eliminar o
/// consultar sus NPCs.
/// </summary>
public class NPCService : INPCService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignNotifier _campaignNotifier;

    public NPCService(IUnitOfWork unitOfWork, ICampaignNotifier campaignNotifier)
    {
        _unitOfWork = unitOfWork;
        _campaignNotifier = campaignNotifier;
    }

    public async Task<Result<List<NPCDto>>> GetByCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<List<NPCDto>>.Fallo("Solo el Dungeon Master de la campaña puede ver sus NPCs.", ResultErrorTipo.SinPermiso);

        var npcs = await _unitOfWork.NPCs.GetByCampaniaAsync(campaniaId, cancellationToken);
        return Result<List<NPCDto>>.Exito(npcs.Select(MapearADto).ToList());
    }

    public async Task<Result<NPCDto>> GetByIdAsync(int npcId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var npc = await _unitOfWork.NPCs.GetByIdWithInventarioAsync(npcId, cancellationToken);
        if (npc is null)
            return Result<NPCDto>.Fallo("NPC no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(npc.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<NPCDto>.Fallo("No tienes permiso para ver este NPC.", ResultErrorTipo.SinPermiso);

        return Result<NPCDto>.Exito(MapearADto(npc));
    }

    public async Task<Result<NPCDto>> CrearAsync(int campaniaId, int dungeonMasterId, CrearNPCDto dto, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<NPCDto>.Fallo("Solo el Dungeon Master de la campaña puede crear NPCs.", ResultErrorTipo.SinPermiso);

        var validacion = ValidarAtributos(dto.Fuerza, dto.Destreza, dto.Constitucion, dto.Inteligencia, dto.Sabiduria, dto.Carisma);
        if (!validacion.EsExitoso)
            return Result<NPCDto>.Fallo(validacion.Error!, validacion.TipoError!.Value);

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<NPCDto>.Fallo("El nombre del NPC es obligatorio.", ResultErrorTipo.Validacion);

        var npc = new NPC
        {
            CampaniaId = campaniaId,
            Nombre = dto.Nombre,
            ImagenUrl = dto.ImagenUrl,
            Nivel = dto.Nivel,
            PuntosVida = dto.PuntosVida,
            PuntosVidaMaximos = dto.PuntosVidaMaximos,
            ClaseArmadura = dto.ClaseArmadura,
            Fuerza = dto.Fuerza,
            Destreza = dto.Destreza,
            Constitucion = dto.Constitucion,
            Inteligencia = dto.Inteligencia,
            Sabiduria = dto.Sabiduria,
            Carisma = dto.Carisma,
            Notas = dto.Notas,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.NPCs.AddAsync(npc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NPCDto>.Exito(MapearADto(npc));
    }

    public async Task<Result<NPCDto>> CrearDesdeCatalogoAsync(int campaniaId, int dungeonMasterId, CrearNpcDesdeCatalogoDto dto, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<NPCDto>.Fallo("Solo el Dungeon Master de la campaña puede crear NPCs.", ResultErrorTipo.SinPermiso);

        var monstruo = await _unitOfWork.MonstruosCatalogo.GetConDetallesAsync(dto.MonstruoId, cancellationToken);
        if (monstruo is null)
            return Result<NPCDto>.Fallo("Monstruo no encontrado en el catálogo.", ResultErrorTipo.NoEncontrado);

        var npc = new NPC
        {
            CampaniaId = campaniaId,
            Nombre = string.IsNullOrWhiteSpace(dto.NombrePersonalizado) ? monstruo.Nombre : dto.NombrePersonalizado.Trim(),
            ImagenUrl = null,
            Nivel = NivelAproximadoDesdeDesafio(monstruo.Desafio),
            PuntosVida = monstruo.PuntosVidaPromedio,
            PuntosVidaMaximos = monstruo.PuntosVidaPromedio,
            ClaseArmadura = monstruo.ClaseArmadura,
            Fuerza = monstruo.Fuerza,
            Destreza = monstruo.Destreza,
            Constitucion = monstruo.Constitucion,
            Inteligencia = monstruo.Inteligencia,
            Sabiduria = monstruo.Sabiduria,
            Carisma = monstruo.Carisma,
            Notas = FormatearNotasDesdeMonstruo(monstruo),
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.NPCs.AddAsync(npc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NPCDto>.Exito(MapearADto(npc));
    }

    /// <summary>
    /// El NPC no tiene un campo "desafío", solo "Nivel" (entero). Un
    /// CR fraccionario (ej. "1/8") se trata como nivel 1 (amenaza
    /// menor); un CR entero se usa tal cual, con mínimo 1.
    /// </summary>
    private static int NivelAproximadoDesdeDesafio(string desafio)
    {
        if (desafio.Contains('/')) return 1;
        return int.TryParse(desafio, out var nivel) ? Math.Max(1, nivel) : 1;
    }

    /// <summary>
    /// El NPC no modela tipo/tamaño/salvaciones/rasgos/acciones como
    /// campos propios (a diferencia de MonstruoCatalogo), así que se
    /// vuelcan aquí como texto en Notas para no perder esa
    /// información al copiar el monstruo del catálogo a la campaña.
    /// </summary>
    private static string FormatearNotasDesdeMonstruo(MonstruoCatalogo monstruo)
    {
        var lineas = new List<string>
        {
            $"{monstruo.Tamano} {monstruo.Tipo}{(string.IsNullOrWhiteSpace(monstruo.Alineamiento) ? "" : $", {monstruo.Alineamiento}")}",
            $"Desafío {monstruo.Desafio} ({monstruo.PuntosExperiencia} PX) · Velocidad {monstruo.Velocidad} · Dado de golpe {monstruo.DadoGolpe}",
        };

        if (!string.IsNullOrWhiteSpace(monstruo.Salvaciones)) lineas.Add($"Salvaciones: {monstruo.Salvaciones}");
        if (!string.IsNullOrWhiteSpace(monstruo.Habilidades)) lineas.Add($"Habilidades: {monstruo.Habilidades}");
        if (!string.IsNullOrWhiteSpace(monstruo.ResistenciasDano)) lineas.Add($"Resistencias al daño: {monstruo.ResistenciasDano}");
        if (!string.IsNullOrWhiteSpace(monstruo.InmunidadesDano)) lineas.Add($"Inmunidades al daño: {monstruo.InmunidadesDano}");
        if (!string.IsNullOrWhiteSpace(monstruo.InmunidadesEstado)) lineas.Add($"Inmunidades a estados: {monstruo.InmunidadesEstado}");
        if (!string.IsNullOrWhiteSpace(monstruo.Sentidos)) lineas.Add($"Sentidos: {monstruo.Sentidos}");
        if (!string.IsNullOrWhiteSpace(monstruo.Idiomas)) lineas.Add($"Idiomas: {monstruo.Idiomas}");

        lineas.Add(""); // separador antes de rasgos/acciones

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
        }

        lineas.Add("");
        lineas.Add($"(Copiado del catálogo precreado: {monstruo.Nombre}, fuente {monstruo.Fuente})");

        return string.Join("\n", lineas);
    }

    public async Task<Result<NPCDto>> EditarAsync(int npcId, int dungeonMasterId, EditarNPCDto dto, CancellationToken cancellationToken = default)
    {
        var npc = await _unitOfWork.NPCs.GetByIdAsync(npcId, cancellationToken);
        if (npc is null)
            return Result<NPCDto>.Fallo("NPC no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(npc.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<NPCDto>.Fallo("No tienes permiso para editar este NPC.", ResultErrorTipo.SinPermiso);

        var validacion = ValidarAtributos(dto.Fuerza, dto.Destreza, dto.Constitucion, dto.Inteligencia, dto.Sabiduria, dto.Carisma);
        if (!validacion.EsExitoso)
            return Result<NPCDto>.Fallo(validacion.Error!, validacion.TipoError!.Value);

        npc.Nombre = dto.Nombre;
        npc.ImagenUrl = dto.ImagenUrl;
        npc.Nivel = dto.Nivel;
        npc.PuntosVida = dto.PuntosVida;
        npc.PuntosVidaMaximos = dto.PuntosVidaMaximos;
        npc.ClaseArmadura = dto.ClaseArmadura;
        npc.Fuerza = dto.Fuerza;
        npc.Destreza = dto.Destreza;
        npc.Constitucion = dto.Constitucion;
        npc.Inteligencia = dto.Inteligencia;
        npc.Sabiduria = dto.Sabiduria;
        npc.Carisma = dto.Carisma;
        npc.Notas = dto.Notas;

        _unitOfWork.NPCs.Update(npc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarNPCActualizadoAsync(npc.CampaniaId, npc.Id, cancellationToken);

        return Result<NPCDto>.Exito(MapearADto(npc));
    }

    public async Task<Result> EliminarAsync(int npcId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var npc = await _unitOfWork.NPCs.GetByIdWithInventarioAsync(npcId, cancellationToken);
        if (npc is null)
            return Result.Fallo("NPC no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(npc.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result.Fallo("No tienes permiso para eliminar este NPC.", ResultErrorTipo.SinPermiso);

        // La FK NPC->Inventario es Restrict (ver InventarioItemConfiguration),
        // por lo que el inventario del NPC debe limpiarse explícitamente
        // antes de eliminar el NPC para no violar la restricción.
        foreach (var item in npc.Inventario.ToList())
        {
            npc.Inventario.Remove(item);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _unitOfWork.NPCs.Remove(npc);
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

    private static NPCDto MapearADto(NPC n) => new(
        n.Id,
        n.CampaniaId,
        n.Nombre,
        n.ImagenUrl,
        n.Nivel,
        n.PuntosVida,
        n.PuntosVidaMaximos,
        n.ClaseArmadura,
        n.Fuerza,
        n.Destreza,
        n.Constitucion,
        n.Inteligencia,
        n.Sabiduria,
        n.Carisma,
        n.ModificadorFuerza,
        n.ModificadorDestreza,
        n.ModificadorConstitucion,
        n.ModificadorInteligencia,
        n.ModificadorSabiduria,
        n.ModificadorCarisma,
        n.Notas);
}
