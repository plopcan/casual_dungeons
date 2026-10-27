using CasualDungeons.Domain.Common;

namespace CasualDungeons.Domain.Entities;

/// <summary>
/// Usuario del sistema. Tabla dnd.Usuarios.
/// La contraseña se almacena siempre como hash (BCrypt), nunca en
/// texto plano: PasswordHash es responsabilidad de Infrastructure
/// generarlo/verificarlo, el dominio solo lo transporta.
/// </summary>
public class Usuario : BaseEntity
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? UltimoAcceso { get; set; }

    public Rol Rol { get; set; } = null!;
    public ConfiguracionUsuario? Configuracion { get; set; }

    public ICollection<Campania> CampaniasComoDM { get; set; } = new List<Campania>();
    public ICollection<ParticipanteCampania> Participaciones { get; set; } = new List<ParticipanteCampania>();
    public ICollection<Personaje> Personajes { get; set; } = new List<Personaje>();
}
