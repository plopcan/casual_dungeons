namespace CasualDungeons.Application.DTOs.NPCs;

public record NPCDto(
    int Id,
    int CampaniaId,
    string Nombre,
    string? ImagenUrl,
    int Nivel,
    int PuntosVida,
    int PuntosVidaMaximos,
    int ClaseArmadura,
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
    string? Notas);

public record CrearNPCDto(
    string Nombre,
    string? ImagenUrl,
    int Nivel,
    int PuntosVida,
    int PuntosVidaMaximos,
    int ClaseArmadura,
    int Fuerza,
    int Destreza,
    int Constitucion,
    int Inteligencia,
    int Sabiduria,
    int Carisma,
    string? Notas);

public record EditarNPCDto(
    string Nombre,
    string? ImagenUrl,
    int Nivel,
    int PuntosVida,
    int PuntosVidaMaximos,
    int ClaseArmadura,
    int Fuerza,
    int Destreza,
    int Constitucion,
    int Inteligencia,
    int Sabiduria,
    int Carisma,
    string? Notas);
