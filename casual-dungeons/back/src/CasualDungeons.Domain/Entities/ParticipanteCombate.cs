using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Participante en el orden de iniciativa de un combate.
/// Tabla dnd.ParticipantesCombate.
///
/// Referencia exactamente uno de PersonajeId, NPCId o
/// MonstruoCampaniaId (nunca más de uno ni ninguno), reflejando el
/// CHECK constraint de la base de datos. El orden se persiste en
/// OrdenIniciativa y se reescribe al reordenar mediante drag & drop
/// desde el frontend.
/// </summary>
public class ParticipanteCombate : BaseEntity
{
    public int CombateId { get; set; }
    public int? PersonajeId { get; set; }
    public int? NPCId { get; set; }
    public int? MonstruoCampaniaId { get; set; }
    public int OrdenIniciativa { get; set; }

    /// <summary>Resultado de la tirada 1d20 + modificador de Destreza.</summary>
    public int? ValorIniciativa { get; set; }

    /// <summary>Snapshot editable de PV durante el combate.</summary>
    public int? PuntosVidaActuales { get; set; }

    public Combate Combate { get; set; } = null!;
    public Personaje? Personaje { get; set; }
    public NPC? NPC { get; set; }
    public MonstruoCampania? MonstruoCampania { get; set; }

    /// <summary>Nombre a mostrar, independientemente del origen (PC, NPC o monstruo).</summary>
    public string NombreParticipante => Personaje?.Nombre ?? NPC?.Nombre ?? MonstruoCampania?.Nombre ?? "Desconocido";
}
