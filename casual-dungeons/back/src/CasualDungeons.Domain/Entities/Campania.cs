using CasualDungeons.Domain.Common;
using CasualDungeons.Domain.Enums;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Campaña de D&D 5e. Tabla dnd.Campanias.
/// Propiedad de un único Dungeon Master; puede tener múltiples
/// jugadores participantes (relación N:M vía ParticipanteCampania).
/// </summary>
public class Campania : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFinalizacion { get; set; }
    public EstadoCampania Estado { get; set; } = EstadoCampania.Activa;
    public int DungeonMasterId { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Usuario DungeonMaster { get; set; } = null!;
    public ICollection<ParticipanteCampania> Participantes { get; set; } = new List<ParticipanteCampania>();
    public ICollection<Personaje> Personajes { get; set; } = new List<Personaje>();
    public ICollection<NPC> NPCs { get; set; } = new List<NPC>();
    public ICollection<MonstruoCampania> MonstruosCampania { get; set; } = new List<MonstruoCampania>();
    public ICollection<Combate> Combates { get; set; } = new List<Combate>();
}
