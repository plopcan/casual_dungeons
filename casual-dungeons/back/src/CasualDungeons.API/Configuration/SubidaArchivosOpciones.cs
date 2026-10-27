namespace CasualDungeons.API.Configuration;

/// <summary>
/// Constantes de validación para la subida de imágenes, compartidas
/// entre el controlador (validación) y Program.cs (límite de Kestrel).
/// Centralizadas aquí para no duplicar "5MB" o la lista de
/// extensiones permitidas en varios sitios.
/// </summary>
public static class SubidaArchivosOpciones
{
    public const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    public static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public static readonly HashSet<string> MimeTypesPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public const string CarpetaImagenes = "imagenes";
}
