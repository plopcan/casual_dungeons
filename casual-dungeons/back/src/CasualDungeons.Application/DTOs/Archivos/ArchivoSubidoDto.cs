namespace CasualDungeons.Application.DTOs.Archivos;

/// <summary>
/// Resultado de una subida de archivo exitosa. Url es siempre una
/// ruta relativa al origen del backend (ej. "/uploads/imagenes/{guid}.png"),
/// nunca una URL absoluta: el frontend decide contra qué origen resolverla.
/// </summary>
public record ArchivoSubidoDto(string Url);
