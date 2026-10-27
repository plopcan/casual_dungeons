namespace CasualDungeons.Application.DTOs.Monstruos;

/// <summary>Fila del selector: lo mínimo para elegir un monstruo de una lista.</summary>
public record MonstruoCatalogoResumenDto(
    int Id,
    string Nombre,
    string Tipo,
    string Tamano,
    string Desafio,
    int PuntosVidaPromedio,
    int ClaseArmadura);

/// <summary>Bloque con nombre del stat block (rasgo, acción, acción legendaria o reacción).</summary>
public record MonstruoCatalogoDetalleItemDto(string Tipo, string Nombre, string Descripcion);

/// <summary>Stat block completo, para previsualizar antes de añadirlo a la campaña.</summary>
public record MonstruoCatalogoFichaDto(
    int Id,
    string Nombre,
    string Tipo,
    string Tamano,
    string? Alineamiento,
    int ClaseArmadura,
    string? DescripcionArmadura,
    int PuntosVidaPromedio,
    string DadoGolpe,
    string Velocidad,
    int Fuerza,
    int Destreza,
    int Constitucion,
    int Inteligencia,
    int Sabiduria,
    int Carisma,
    string? Salvaciones,
    string? Habilidades,
    string? ResistenciasDano,
    string? InmunidadesDano,
    string? InmunidadesEstado,
    string? Sentidos,
    string? Idiomas,
    string Desafio,
    int PuntosExperiencia,
    string Fuente,
    List<MonstruoCatalogoDetalleItemDto> Detalles);

/// <summary>
/// Petición para añadir un monstruo del catálogo a una campaña como
/// NPC. NombrePersonalizado es opcional (ej. "Goblin explorador");
/// si se omite, se usa el nombre del monstruo tal cual.
/// </summary>
public record CrearNpcDesdeCatalogoDto(int MonstruoId, string? NombrePersonalizado);
