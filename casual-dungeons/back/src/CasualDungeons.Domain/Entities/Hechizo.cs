using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Catálogo general de hechizos de 5e. Tabla dnd.Hechizos.
/// Nivel 0 = truco (cantrip); 1-9 = nivel de conjuro.
/// </summary>
public class Hechizo : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public string? Escuela { get; set; }
    public string? TiempoLanzamiento { get; set; }
    public string? Alcance { get; set; }
    public string? Componentes { get; set; }
    public string? Duracion { get; set; }
    public string? Descripcion { get; set; }

    public ICollection<PersonajeHechizo> PersonajeHechizos { get; set; } = new List<PersonajeHechizo>();
}
