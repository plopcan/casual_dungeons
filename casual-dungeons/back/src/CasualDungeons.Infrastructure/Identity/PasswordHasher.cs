using CasualDungeons.Application.Interfaces;

namespace CasualDungeons.Infrastructure.Identity;

/// <summary>
/// Hash de contraseñas mediante BCrypt (paquete BCrypt.Net-Next).
/// El factor de coste 12 es el recomendado por la librería para
/// un balance entre seguridad y rendimiento en 2025+.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
