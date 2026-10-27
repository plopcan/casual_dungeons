namespace CasualDungeons.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio. Centraliza
/// la clave primaria para evitar repetirla en cada entidad.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
