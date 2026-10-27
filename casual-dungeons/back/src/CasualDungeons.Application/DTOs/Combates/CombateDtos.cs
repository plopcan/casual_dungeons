namespace CasualDungeons.Application.DTOs.Combates;

public record CombateDto(
    int Id,
    int CampaniaId,
    string Estado,
    DateTime Fecha,
    int RondaActual,
    List<ParticipanteCombateDto> Participantes);

public record ParticipanteCombateDto(
    int Id,
    int? PersonajeId,
    int? NPCId,
    int? MonstruoCampaniaId,
    string Nombre,
    int OrdenIniciativa,
    int? ValorIniciativa,
    int? PuntosVidaActuales,
    string Origen); // "Personaje", "NPC" o "MonstruoCampania"

public record CrearCombateDto(int CampaniaId);

public record AgregarParticipanteCombateDto(int? PersonajeId, int? NPCId, int? MonstruoCampaniaId, int? ValorIniciativa);

/// <summary>
/// Nuevo orden de participantes tras un drag &amp; drop en el frontend.
/// La lista debe contener TODOS los Id de ParticipanteCombate del
/// combate, en el orden deseado (índice 0 = primero en iniciativa).
/// </summary>
public record ReordenarParticipantesDto(List<int> ParticipanteCombateIdsEnOrden);
