using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Combates;
using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using CasualDungeons.Domain.Enums;

namespace CasualDungeons.Application.Services;

/// <summary>
/// Gestión de combates e iniciativa. Todas las operaciones de
/// escritura están restringidas al Dungeon Master propietario de
/// la campaña a la que pertenece el combate.
/// </summary>
public class CombateService : ICombateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignNotifier _campaignNotifier;

    public CombateService(IUnitOfWork unitOfWork, ICampaignNotifier campaignNotifier)
    {
        _unitOfWork = unitOfWork;
        _campaignNotifier = campaignNotifier;
    }

    public async Task<Result<List<CombateDto>>> GetByCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(campaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<List<CombateDto>>.Fallo("Solo el Dungeon Master de la campaña puede ver los combates.", ResultErrorTipo.SinPermiso);

        var combates = await _unitOfWork.Combates.GetByCampaniaAsync(campaniaId, cancellationToken);
        return Result<List<CombateDto>>.Exito(combates.Select(MapearADto).ToList());
    }

    public async Task<Result<CombateDto>> GetByIdAsync(int combateId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var combate = await _unitOfWork.Combates.GetByIdWithParticipantesAsync(combateId, cancellationToken);
        if (combate is null)
            return Result<CombateDto>.Fallo("Combate no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(combate.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<CombateDto>.Fallo("No tienes permiso para ver este combate.", ResultErrorTipo.SinPermiso);

        return Result<CombateDto>.Exito(MapearADto(combate));
    }

    public async Task<Result<CombateDto>> CrearAsync(CrearCombateDto dto, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(dto.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<CombateDto>.Fallo("Solo el Dungeon Master de la campaña puede crear combates.", ResultErrorTipo.SinPermiso);

        var combate = new Combate
        {
            CampaniaId = dto.CampaniaId,
            Estado = EstadoCombate.EnPreparacion,
            Fecha = DateTime.UtcNow,
            RondaActual = 1
        };

        await _unitOfWork.Combates.AddAsync(combate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CombateDto>.Exito(MapearADto(combate));
    }

    public async Task<Result<CombateDto>> AgregarParticipanteAsync(int combateId, int dungeonMasterId, AgregarParticipanteCombateDto dto, CancellationToken cancellationToken = default)
    {
        var combate = await _unitOfWork.Combates.GetByIdWithParticipantesAsync(combateId, cancellationToken);
        if (combate is null)
            return Result<CombateDto>.Fallo("Combate no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(combate.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<CombateDto>.Fallo("No tienes permiso para modificar este combate.", ResultErrorTipo.SinPermiso);

        var esPersonaje = dto.PersonajeId.HasValue;
        var esNPC = dto.NPCId.HasValue;
        var esMonstruo = dto.MonstruoCampaniaId.HasValue;

        if (new[] { esPersonaje, esNPC, esMonstruo }.Count(x => x) != 1)
            return Result<CombateDto>.Fallo("Debe indicarse exactamente un personaje, un NPC o un monstruo, no varios ni ninguno.", ResultErrorTipo.Validacion);

        int? puntosVidaIniciales;

        if (esPersonaje)
        {
            var personaje = await _unitOfWork.Personajes.GetByIdAsync(dto.PersonajeId!.Value, cancellationToken);
            if (personaje is null)
                return Result<CombateDto>.Fallo("Personaje no encontrado.", ResultErrorTipo.NoEncontrado);
            if (personaje.CampaniaId != combate.CampaniaId)
                return Result<CombateDto>.Fallo("El personaje no pertenece a la campaña de este combate.", ResultErrorTipo.Validacion);
            if (combate.Participantes.Any(p => p.PersonajeId == personaje.Id))
                return Result<CombateDto>.Fallo("Ese personaje ya está en el combate.", ResultErrorTipo.Conflicto);

            puntosVidaIniciales = personaje.PuntosVidaActuales;
        }
        else if (esNPC)
        {
            var npc = await _unitOfWork.NPCs.GetByIdAsync(dto.NPCId!.Value, cancellationToken);
            if (npc is null)
                return Result<CombateDto>.Fallo("NPC no encontrado.", ResultErrorTipo.NoEncontrado);
            if (npc.CampaniaId != combate.CampaniaId)
                return Result<CombateDto>.Fallo("El NPC no pertenece a la campaña de este combate.", ResultErrorTipo.Validacion);
            if (combate.Participantes.Any(p => p.NPCId == npc.Id))
                return Result<CombateDto>.Fallo("Ese NPC ya está en el combate.", ResultErrorTipo.Conflicto);

            puntosVidaIniciales = npc.PuntosVida;
        }
        else
        {
            var monstruo = await _unitOfWork.MonstruosCampania.GetByIdAsync(dto.MonstruoCampaniaId!.Value, cancellationToken);
            if (monstruo is null)
                return Result<CombateDto>.Fallo("Monstruo no encontrado.", ResultErrorTipo.NoEncontrado);
            if (monstruo.CampaniaId != combate.CampaniaId)
                return Result<CombateDto>.Fallo("El monstruo no pertenece a la campaña de este combate.", ResultErrorTipo.Validacion);
            if (combate.Participantes.Any(p => p.MonstruoCampaniaId == monstruo.Id))
                return Result<CombateDto>.Fallo("Ese monstruo ya está en el combate.", ResultErrorTipo.Conflicto);

            puntosVidaIniciales = monstruo.PuntosVidaActuales;
        }

        var siguienteOrden = combate.Participantes.Count == 0
            ? 0
            : combate.Participantes.Max(p => p.OrdenIniciativa) + 1;

        var participante = new ParticipanteCombate
        {
            CombateId = combateId,
            PersonajeId = dto.PersonajeId,
            NPCId = dto.NPCId,
            MonstruoCampaniaId = dto.MonstruoCampaniaId,
            OrdenIniciativa = siguienteOrden,
            ValorIniciativa = dto.ValorIniciativa,
            PuntosVidaActuales = puntosVidaIniciales
        };

        combate.Participantes.Add(participante);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarCombateActualizadoAsync(combate.CampaniaId, combate.Id, cancellationToken);

        var combateActualizado = await _unitOfWork.Combates.GetByIdWithParticipantesAsync(combateId, cancellationToken);
        return Result<CombateDto>.Exito(MapearADto(combateActualizado!));
    }

    public async Task<Result> EliminarParticipanteAsync(int combateId, int participanteCombateId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var combate = await _unitOfWork.Combates.GetByIdWithParticipantesAsync(combateId, cancellationToken);
        if (combate is null)
            return Result.Fallo("Combate no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(combate.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result.Fallo("No tienes permiso para modificar este combate.", ResultErrorTipo.SinPermiso);

        var participante = combate.Participantes.FirstOrDefault(p => p.Id == participanteCombateId);
        if (participante is null)
            return Result.Fallo("Ese participante no está en el combate.", ResultErrorTipo.NoEncontrado);

        combate.Participantes.Remove(participante);

        // Renumerar el orden de iniciativa para no dejar huecos,
        // requerido por el índice único (CombateId, OrdenIniciativa).
        var restantesOrdenados = combate.Participantes.OrderBy(p => p.OrdenIniciativa).ToList();
        for (var i = 0; i < restantesOrdenados.Count; i++)
        {
            restantesOrdenados[i].OrdenIniciativa = i;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarCombateActualizadoAsync(combate.CampaniaId, combate.Id, cancellationToken);

        return Result.Exito();
    }

    public async Task<Result<CombateDto>> ReordenarParticipantesAsync(int combateId, int dungeonMasterId, ReordenarParticipantesDto dto, CancellationToken cancellationToken = default)
    {
        var combate = await _unitOfWork.Combates.GetByIdWithParticipantesAsync(combateId, cancellationToken);
        if (combate is null)
            return Result<CombateDto>.Fallo("Combate no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(combate.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result<CombateDto>.Fallo("No tienes permiso para modificar este combate.", ResultErrorTipo.SinPermiso);

        var idsActuales = combate.Participantes.Select(p => p.Id).ToHashSet();
        var idsSolicitados = dto.ParticipanteCombateIdsEnOrden.ToHashSet();

        if (!idsActuales.SetEquals(idsSolicitados))
            return Result<CombateDto>.Fallo("La lista de orden debe contener exactamente todos los participantes del combate.", ResultErrorTipo.Validacion);

        // Se reescribe OrdenIniciativa según la posición en la lista
        // recibida del frontend (resultado del drag & drop).
        for (var i = 0; i < dto.ParticipanteCombateIdsEnOrden.Count; i++)
        {
            var participante = combate.Participantes.First(p => p.Id == dto.ParticipanteCombateIdsEnOrden[i]);
            participante.OrdenIniciativa = i;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarCombateActualizadoAsync(combate.CampaniaId, combate.Id, cancellationToken);

        var combateActualizado = await _unitOfWork.Combates.GetByIdWithParticipantesAsync(combateId, cancellationToken);
        return Result<CombateDto>.Exito(MapearADto(combateActualizado!));
    }

    public async Task<Result> FinalizarAsync(int combateId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var combate = await _unitOfWork.Combates.GetByIdAsync(combateId, cancellationToken);
        if (combate is null)
            return Result.Fallo("Combate no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(combate.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result.Fallo("No tienes permiso para modificar este combate.", ResultErrorTipo.SinPermiso);

        combate.Estado = EstadoCombate.Finalizado;
        _unitOfWork.Combates.Update(combate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignNotifier.NotificarCombateActualizadoAsync(combate.CampaniaId, combate.Id, cancellationToken);

        return Result.Exito();
    }

    public async Task<Result> EliminarAsync(int combateId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var combate = await _unitOfWork.Combates.GetByIdAsync(combateId, cancellationToken);
        if (combate is null)
            return Result.Fallo("Combate no encontrado.", ResultErrorTipo.NoEncontrado);

        var esDM = await _unitOfWork.Campanias.EsDungeonMasterAsync(combate.CampaniaId, dungeonMasterId, cancellationToken);
        if (!esDM)
            return Result.Fallo("No tienes permiso para eliminar este combate.", ResultErrorTipo.SinPermiso);

        // La cascada Combate->ParticipanteCombate elimina automáticamente
        // a los participantes.
        _unitOfWork.Combates.Remove(combate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    private static CombateDto MapearADto(Combate combate) => new(
        combate.Id,
        combate.CampaniaId,
        combate.Estado.ToString(),
        combate.Fecha,
        combate.RondaActual,
        combate.Participantes
            .OrderBy(p => p.OrdenIniciativa)
            .Select(p => new ParticipanteCombateDto(
                p.Id,
                p.PersonajeId,
                p.NPCId,
                p.MonstruoCampaniaId,
                p.NombreParticipante,
                p.OrdenIniciativa,
                p.ValorIniciativa,
                p.PuntosVidaActuales,
                p.PersonajeId.HasValue ? "Personaje" : p.NPCId.HasValue ? "NPC" : "MonstruoCampania"))
            .ToList());
}
