using CasualDungeons.Domain.Common;
using CasualDungeons.Domain.Enums;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Un bloque con nombre del stat block de un monstruo (rasgo,
/// acción, acción legendaria o reacción). Tabla
/// dnd.MonstruosCatalogoDetalles. Ej: Tipo=Accion,
/// Nombre="Mordisco", Descripcion="Ataque de arma cuerpo a cuerpo...".
/// </summary>
public class MonstruoCatalogoDetalle : BaseEntity
{
    public int MonstruoId { get; set; }
    public TipoDetalleMonstruo Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Orden de aparición dentro de su bloque (Rasgos, luego Acciones, etc.).</summary>
    public int Orden { get; set; }

    public MonstruoCatalogo Monstruo { get; set; } = null!;
}
