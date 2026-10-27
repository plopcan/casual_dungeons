using CasualDungeons.Domain.Common;
using CasualDungeons.Domain.Enums;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Encuentro de combate de una campaña. Tabla dnd.Combates.
/// </summary>
public class Combate : BaseEntity
{
    public int CampaniaId { get; set; }
    public EstadoCombate Estado { get; set; } = EstadoCombate.EnPreparacion;
    public DateTime Fecha { get; set; }
    public int RondaActual { get; set; } = 1;

    public Campania Campania { get; set; } = null!;
    public ICollection<ParticipanteCombate> Participantes { get; set; } = new List<ParticipanteCombate>();
}
