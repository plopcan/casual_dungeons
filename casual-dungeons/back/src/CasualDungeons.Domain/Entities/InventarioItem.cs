using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Ítem de inventario y equipo. Tabla dnd.Inventario.
/// Pertenece exactamente a un Personaje o a un NPC (nunca ambos
/// ni ninguno), reflejando el CHECK constraint de la base de datos.
/// </summary>
public class InventarioItem : BaseEntity
{
    public int? PersonajeId { get; set; }
    public int? NPCId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal? Peso { get; set; }
    public bool Equipado { get; set; }
    public string? Tipo { get; set; }

    public Personaje? Personaje { get; set; }
    public NPC? NPC { get; set; }
}
