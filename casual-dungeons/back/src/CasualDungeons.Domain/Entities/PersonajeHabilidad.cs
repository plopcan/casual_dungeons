using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Habilidad (skill) de 5e asociada a un personaje, con su estado
/// de competencia/experticia. Tabla dnd.PersonajeHabilidades.
/// </summary>
public class PersonajeHabilidad : BaseEntity
{
    public int PersonajeId { get; set; }
    public string NombreHabilidad { get; set; } = string.Empty;
    public string AtributoBase { get; set; } = string.Empty;
    public bool Competente { get; set; }
    public bool Experticia { get; set; }

    public Personaje Personaje { get; set; } = null!;
}
