using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Personaje no jugador (NPC), propiedad del Dungeon Master.
/// Tabla dnd.NPCs. Comparte las reglas de atributos 5e con Personaje.
/// </summary>
public class NPC : BaseEntity
{
    public int CampaniaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public int Nivel { get; set; } = 1;
    public int PuntosVida { get; set; } = 1;
    public int PuntosVidaMaximos { get; set; } = 1;
    public int ClaseArmadura { get; set; } = 10;

    public int Fuerza { get; set; } = 10;
    public int Destreza { get; set; } = 10;
    public int Constitucion { get; set; } = 10;
    public int Inteligencia { get; set; } = 10;
    public int Sabiduria { get; set; } = 10;
    public int Carisma { get; set; } = 10;

    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Campania Campania { get; set; } = null!;
    public ICollection<InventarioItem> Inventario { get; set; } = new List<InventarioItem>();

    public int ModificadorFuerza => Personaje.CalcularModificador(Fuerza);
    public int ModificadorDestreza => Personaje.CalcularModificador(Destreza);
    public int ModificadorConstitucion => Personaje.CalcularModificador(Constitucion);
    public int ModificadorInteligencia => Personaje.CalcularModificador(Inteligencia);
    public int ModificadorSabiduria => Personaje.CalcularModificador(Sabiduria);
    public int ModificadorCarisma => Personaje.CalcularModificador(Carisma);
}
