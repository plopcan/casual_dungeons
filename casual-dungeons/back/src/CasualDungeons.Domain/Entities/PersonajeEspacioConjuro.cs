using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Espacios de conjuro disponibles/usados por nivel (1-9) para un
/// personaje. Tabla dnd.PersonajeEspaciosConjuro.
/// </summary>
public class PersonajeEspacioConjuro : BaseEntity
{
    public int PersonajeId { get; set; }
    public int NivelConjuro { get; set; }
    public int EspaciosTotales { get; set; }
    public int EspaciosUsados { get; set; }

    public Personaje Personaje { get; set; } = null!;

    public int EspaciosDisponibles => EspaciosTotales - EspaciosUsados;
}
