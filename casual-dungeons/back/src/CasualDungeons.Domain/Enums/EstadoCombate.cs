namespace CasualDungeons.Domain.Enums;

/// <summary>
/// Estado de un encuentro de combate. Coincide con el CHECK
/// constraint CK_Combates_Estado de la base de datos.
/// </summary>
public enum EstadoCombate
{
    EnPreparacion,
    EnCurso,
    Finalizado
}
