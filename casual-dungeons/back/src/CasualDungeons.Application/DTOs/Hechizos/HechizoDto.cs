namespace CasualDungeons.Application.DTOs.Hechizos;

public record HechizoDto(
    int Id,
    string Nombre,
    int Nivel,
    string? Escuela,
    string? TiempoLanzamiento,
    string? Alcance,
    string? Componentes,
    string? Duracion,
    string? Descripcion);
