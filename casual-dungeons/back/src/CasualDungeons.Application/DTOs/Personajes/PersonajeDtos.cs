namespace CasualDungeons.Application.DTOs.Personajes;

/// <summary>
/// Resumen de personaje para tarjetas (vista DM - pestaña Jugadores).
/// </summary>
public record PersonajeResumenDto(
    int Id,
    string NombrePersonaje,
    string NombreJugador,
    string Clase,
    int Nivel,
    int PuntosVidaActuales,
    int PuntosVidaMaximos,
    int ClaseArmadura,
    int PercepcionPasiva,
    bool Activo);

/// <summary>
/// Ficha completa del personaje, organizada en las mismas pestañas
/// que el frontend: información general, estadísticas, combate,
/// inventario, habilidades, hechizos, notas.
/// </summary>
public record PersonajeFichaDto(
    int Id,
    int CampaniaId,
    int UsuarioId,

    // Información general
    string Nombre,
    string Raza,
    string Clase,
    string? Subclase,
    string? Trasfondo,
    string? Alineamiento,
    string? ImagenUrl,
    int Nivel,
    int Experiencia,

    // Estadísticas (atributos + modificadores calculados)
    int Fuerza,
    int Destreza,
    int Constitucion,
    int Inteligencia,
    int Sabiduria,
    int Carisma,
    int ModificadorFuerza,
    int ModificadorDestreza,
    int ModificadorConstitucion,
    int ModificadorInteligencia,
    int ModificadorSabiduria,
    int ModificadorCarisma,
    int BonificadorCompetencia,

    // Combate
    int PuntosVidaActuales,
    int PuntosVidaMaximos,
    int PuntosVidaTemporales,
    string? DadoGolpe,
    int DadosGolpeRestantes,
    int ClaseArmadura,
    int Velocidad,
    int IniciativaBonus,
    int PercepcionPasiva,

    // Colecciones (inventario, habilidades, hechizos, notas)
    List<InventarioItemDto> Inventario,
    List<PersonajeHabilidadDto> Habilidades,
    List<PersonajeHechizoDto> Hechizos,
    List<PersonajeEspacioConjuroDto> EspaciosConjuro,
    List<NotaDto> Notas,

    string? NotasGenerales,
    bool Activo,
    bool SoloLectura);

public record ActualizarPersonajeDto(
    string Nombre,
    string Raza,
    string Clase,
    string? Subclase,
    string? Trasfondo,
    string? Alineamiento,
    string? ImagenUrl,
    int Nivel,
    int Experiencia,
    int Fuerza,
    int Destreza,
    int Constitucion,
    int Inteligencia,
    int Sabiduria,
    int Carisma,
    int PuntosVidaActuales,
    int PuntosVidaMaximos,
    int PuntosVidaTemporales,
    string? DadoGolpe,
    int DadosGolpeRestantes,
    int ClaseArmadura,
    int Velocidad,
    int IniciativaBonus,
    string? NotasGenerales);

/// <summary>
/// Datos para crear un personaje nuevo (nivel 1). Solo se piden
/// los datos de concepto del personaje; los valores derivados
/// (puntos de golpe, CA, iniciativa, percepción pasiva) se calculan
/// en el servidor según las reglas de D&D 5e a partir de la clase y
/// los atributos. Los atributos son opcionales: si no se indican,
/// se usa 10 (modificador +0) en todos.
/// </summary>
public record CrearPersonajeDto(
    string Nombre,
    string Raza,
    string Clase,
    string? Subclase,
    string? Trasfondo,
    string? Alineamiento,
    string? ImagenUrl,
    int? Fuerza,
    int? Destreza,
    int? Constitucion,
    int? Inteligencia,
    int? Sabiduria,
    int? Carisma);

/// <summary>Confirmación tras activar/desactivar un personaje.</summary>
public record PersonajeActivoDto(int PersonajeId, bool Activo);

public record InventarioItemDto(
    int Id,
    string Nombre,
    string? Descripcion,
    int Cantidad,
    decimal? Peso,
    bool Equipado,
    string? Tipo);

public record GuardarInventarioItemDto(
    string Nombre,
    string? Descripcion,
    int Cantidad,
    decimal? Peso,
    bool Equipado,
    string? Tipo);

public record PersonajeHabilidadDto(
    int Id,
    string NombreHabilidad,
    string AtributoBase,
    bool Competente,
    bool Experticia);

public record ActualizarHabilidadDto(bool Competente, bool Experticia);
public record CambiarActivoDto(bool Activo);
public record ConfigurarEspacioConjuroDto(int EspaciosTotales);

public record PersonajeHechizoDto(
    int Id,
    int HechizoId,
    string NombreHechizo,
    string DescripcionHechizo,
    int NivelHechizo,
    string? Escuela,
    bool Preparado);

public record PersonajeEspacioConjuroDto(
    int NivelConjuro,
    int EspaciosTotales,
    int EspaciosUsados,
    int EspaciosDisponibles);

public record NotaDto(
    int Id,
    string? Titulo,
    string? Contenido,
    DateTime FechaActualizacion);

public record GuardarNotaDto(string? Titulo, string? Contenido);
