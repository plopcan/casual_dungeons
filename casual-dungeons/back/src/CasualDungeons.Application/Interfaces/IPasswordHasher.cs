namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Abstracción sobre el algoritmo de hash de contraseñas (BCrypt).
/// Mantiene Application independiente de la librería concreta.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
