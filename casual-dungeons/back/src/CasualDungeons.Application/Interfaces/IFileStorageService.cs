namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Almacenamiento de archivos subidos por el usuario (imágenes de
/// campañas, NPCs, personajes, etc.). La implementación en
/// Infrastructure guarda en disco local bajo wwwroot/uploads, pero
/// esta abstracción permite sustituirla (ej. por almacenamiento en
/// la nube) sin tocar Application ni API.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda el contenido del stream bajo la carpeta indicada con
    /// un nombre de archivo generado (GUID + extensión original) y
    /// devuelve la ruta relativa desde la que se podrá servir
    /// (ej. "/uploads/imagenes/3f2a1e...-b1.png").
    /// </summary>
    Task<string> GuardarArchivoAsync(string carpeta, Stream contenido, string nombreOriginal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina el archivo referenciado por una ruta relativa
    /// previamente devuelta por GuardarArchivoAsync. No lanza si el
    /// archivo no existe. Protegido contra path traversal: solo
    /// elimina archivos dentro de la carpeta de uploads.
    /// </summary>
    void EliminarArchivo(string? urlRelativa);
}
