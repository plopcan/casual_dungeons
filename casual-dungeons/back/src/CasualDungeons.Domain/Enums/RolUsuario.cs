namespace CasualDungeons.Domain.Enums;

/// <summary>
/// Roles de usuario del sistema. Los valores numéricos coinciden
/// con los Id semilla insertados en el script SQL (dnd.Roles).
/// </summary>
public enum RolUsuario
{
    Administrador = 1,
    DungeonMaster = 2,
    Jugador = 3
}
