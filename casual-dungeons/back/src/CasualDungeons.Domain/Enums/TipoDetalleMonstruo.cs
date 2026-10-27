namespace CasualDungeons.Domain.Enums;

/// <summary>
/// Tipo de bloque dentro de la estadística de un monstruo del
/// catálogo, siguiendo la estructura estándar de un stat block de
/// D&D 5e: Rasgo (traits, van antes de "Acciones"), Accion,
/// AccionLegendaria y Reaccion. Coincide con el CHECK constraint /
/// ENUM de la columna `tipo` en MonstruosCatalogoDetalles.
/// </summary>
public enum TipoDetalleMonstruo
{
    Rasgo,
    Accion,
    AccionLegendaria,
    Reaccion
}
