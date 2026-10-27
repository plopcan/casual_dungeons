namespace CasualDungeons.Infrastructure.Identity;

/// <summary>
/// Opciones de configuración JWT, enlazadas desde la sección
/// "Jwt" de appsettings.json.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiracionMinutos { get; set; } = 120;
}
