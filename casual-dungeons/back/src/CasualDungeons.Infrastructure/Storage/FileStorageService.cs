using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace CasualDungeons.Infrastructure.Storage;

/// <summary>
/// Guarda los archivos subidos en disco, bajo
/// wwwroot/uploads/{carpeta}/{GUID}.ext. Servidos después como
/// estáticos por app.UseStaticFiles() (ver Program.cs), sin pasar
/// por autenticación JWT.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private const string CarpetaRaiz = "uploads";

    private readonly string _webRootPath;

    public FileStorageService(IWebHostEnvironment webHostEnvironment)
    {
        // WebRootPath puede ser null si wwwroot no existe todavía en
        // disco (primer arranque); se resuelve manualmente en ese caso.
        _webRootPath = webHostEnvironment.WebRootPath
            ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
    }

    public async Task<string> GuardarArchivoAsync(string carpeta, Stream contenido, string nombreOriginal, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(nombreOriginal);
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";

        var carpetaDestino = Path.Combine(_webRootPath, CarpetaRaiz, carpeta);
        Directory.CreateDirectory(carpetaDestino);

        var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

        await using var destino = new FileStream(rutaCompleta, FileMode.Create);
        await contenido.CopyToAsync(destino, cancellationToken);

        // Ruta relativa con separadores '/' (formato URL), independiente
        // del separador de directorios del sistema operativo.
        return $"/{CarpetaRaiz}/{carpeta}/{nombreArchivo}";
    }

    public void EliminarArchivo(string? urlRelativa)
    {
        if (string.IsNullOrWhiteSpace(urlRelativa))
            return;

        // Solo se aceptan rutas relativas propias (/uploads/...); las
        // URLs externas (http://, https://) de datos anteriores a esta
        // funcionalidad no corresponden a un archivo local y se ignoran.
        if (!urlRelativa.StartsWith($"/{CarpetaRaiz}/", StringComparison.OrdinalIgnoreCase))
            return;

        // Normaliza y resuelve contra la raíz de uploads, para luego
        // verificar que el resultado sigue dentro de esa raíz: evita
        // que un ".." en la URL permita borrar archivos fuera de
        // wwwroot/uploads (path traversal).
        var raizUploads = Path.Combine(_webRootPath, CarpetaRaiz);
        var rutaRelativaSinBarra = urlRelativa.TrimStart('/')[(CarpetaRaiz.Length + 1)..];
        var rutaCompleta = Path.GetFullPath(Path.Combine(raizUploads, rutaRelativaSinBarra));
        var raizUploadsCompleta = Path.GetFullPath(raizUploads);

        if (!rutaCompleta.StartsWith(raizUploadsCompleta, StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);
    }
}
