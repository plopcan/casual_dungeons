using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Configuración personal de un usuario (tema visual, etc.).
/// Relación 1:1 con Usuario. Tabla dnd.ConfiguracionesUsuario.
/// El modo oscuro es el predeterminado (TemaOscuro = true).
/// </summary>
public class ConfiguracionUsuario : BaseEntity
{
    public int UsuarioId { get; set; }
    public bool TemaOscuro { get; set; } = true;
    public DateTime FechaActualizacion { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
