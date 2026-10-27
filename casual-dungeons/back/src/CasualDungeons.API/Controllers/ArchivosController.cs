using CasualDungeons.API.Configuration;
using CasualDungeons.Application.DTOs.Archivos;
using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

/// <summary>
/// Subida de imágenes (portadas de campaña, etc.) usadas desde el
/// frontend. Cualquier rol autenticado puede subir: la restricción
/// de quién puede asociar la imagen resultante a una entidad
/// concreta (ej. solo el DM de una campaña) la aplica el controlador
/// de esa entidad, no este.
/// </summary>
[ApiController]
[Route("api/archivos")]
[Authorize]
public class ArchivosController : ApiControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public ArchivosController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("imagenes")]
    [RequestSizeLimit(SubidaArchivosOpciones.TamanoMaximoBytes)]
    public async Task<IActionResult> SubirImagen(IFormFile archivo, CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { error = "No se recibió ningún archivo." });

        if (archivo.Length > SubidaArchivosOpciones.TamanoMaximoBytes)
            return BadRequest(new { error = "El archivo supera el tamaño máximo permitido (5 MB)." });

        var extension = Path.GetExtension(archivo.FileName);
        if (string.IsNullOrEmpty(extension) || !SubidaArchivosOpciones.ExtensionesPermitidas.Contains(extension))
            return BadRequest(new { error = "Extensión de archivo no permitida. Se aceptan: jpg, jpeg, png, webp, gif." });

        if (!SubidaArchivosOpciones.MimeTypesPermitidos.Contains(archivo.ContentType))
            return BadRequest(new { error = "Tipo de contenido no permitido." });

        await using var stream = archivo.OpenReadStream();
        var url = await _fileStorageService.GuardarArchivoAsync(
            SubidaArchivosOpciones.CarpetaImagenes, stream, archivo.FileName, cancellationToken);

        return Ok(new ArchivoSubidoDto(url));
    }
}
