namespace CasualDungeons.Application.DTOs.Campanias;

public record CampaniaResumenDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? ImagenUrl,
    DateOnly FechaInicio,
    DateOnly? FechaFinalizacion,
    string Estado,
    bool EsDungeonMaster);

public record CampaniaDetalleDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? ImagenUrl,
    DateOnly FechaInicio,
    DateOnly? FechaFinalizacion,
    string Estado,
    int DungeonMasterId,
    string DungeonMasterNombre,
    List<ParticipanteDto> Participantes);

public record ParticipanteDto(int UsuarioId, string NombreUsuario, int? PersonajeId, string? PersonajeNombre);

public record CrearCampaniaDto(
    string Nombre,
    string? Descripcion,
    string? ImagenUrl,
    DateOnly FechaInicio,
    List<int> JugadoresIds);

public record EditarCampaniaDto(
    string Nombre,
    string? Descripcion,
    string? ImagenUrl,
    DateOnly FechaInicio,
    DateOnly? FechaFinalizacion,
    string Estado,
    List<int> JugadoresIds);
