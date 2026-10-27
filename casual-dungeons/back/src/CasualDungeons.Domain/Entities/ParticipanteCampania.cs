using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Tabla puente de la relación N:M entre Usuario (jugador) y
/// Campania. Tabla dnd.ParticipantesCampania.
/// </summary>
public class ParticipanteCampania : BaseEntity
{
    public int CampaniaId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaIngreso { get; set; }

    public Campania Campania { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
