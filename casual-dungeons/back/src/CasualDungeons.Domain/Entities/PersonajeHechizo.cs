using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Tabla puente N:M entre Personaje y Hechizo, con el estado de
/// preparación. Tabla dnd.PersonajeHechizos.
/// </summary>
public class PersonajeHechizo : BaseEntity
{
    public int PersonajeId { get; set; }
    public int HechizoId { get; set; }
    public bool Preparado { get; set; }

    public Personaje Personaje { get; set; } = null!;
    public Hechizo Hechizo { get; set; } = null!;
}
