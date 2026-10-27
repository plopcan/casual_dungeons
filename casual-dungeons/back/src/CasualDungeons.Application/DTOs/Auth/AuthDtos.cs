namespace CasualDungeons.Application.DTOs.Auth;

public record LoginRequestDto(string NombreUsuario, string Password);

public record LoginResponseDto(
    string Token,
    DateTime ExpiraEn,
    int UsuarioId,
    string NombreUsuario,
    string Rol);

public record CambiarPasswordRequestDto(string PasswordActual, string PasswordNueva);

public record CambiarNombreUsuarioRequestDto(string NuevoNombreUsuario);
