using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Nota estructurada asociada a un personaje. Tabla dnd.Notas.
/// </summary>
public class Nota : BaseEntity
{
    public int PersonajeId { get; set; }
    public string? Titulo { get; set; }
    public string? Contenido { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public Personaje Personaje { get; set; } = null!;
}
