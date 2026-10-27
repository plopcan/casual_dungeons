using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Auth;
using CasualDungeons.Application.Interfaces;

namespace CasualDungeons.Application.Services;

/// <summary>
/// Lógica de autenticación: validación de credenciales, emisión de
/// JWT y gestión de cambio de contraseña/nombre de usuario.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.NombreUsuario, cancellationToken);

        // Mensaje de error genérico para no revelar si el usuario
        // existe o no (mitiga enumeración de cuentas).
        const string mensajeCredencialesInvalidas = "Nombre de usuario o contraseña incorrectos.";

        if (usuario is null || !usuario.Activo)
            return Result<LoginResponseDto>.Fallo(mensajeCredencialesInvalidas, ResultErrorTipo.Validacion);

        if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash))
            return Result<LoginResponseDto>.Fallo(mensajeCredencialesInvalidas, ResultErrorTipo.Validacion);

        var (token, expiraEn) = _jwtTokenGenerator.GenerarToken(usuario);

        usuario.UltimoAcceso = DateTime.UtcNow;
        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto(
            token,
            expiraEn,
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Rol.Nombre);

        return Result<LoginResponseDto>.Exito(response);
    }

    public async Task<Result> CambiarPasswordAsync(int usuarioId, CambiarPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Result.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        if (!_passwordHasher.Verify(request.PasswordActual, usuario.PasswordHash))
            return Result.Fallo("La contraseña actual no es correcta.", ResultErrorTipo.Validacion);

        if (string.IsNullOrWhiteSpace(request.PasswordNueva) || request.PasswordNueva.Length < 8)
            return Result.Fallo("La nueva contraseña debe tener al menos 8 caracteres.", ResultErrorTipo.Validacion);

        usuario.PasswordHash = _passwordHasher.Hash(request.PasswordNueva);
        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    public async Task<Result> CambiarNombreUsuarioAsync(int usuarioId, CambiarNombreUsuarioRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Result.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        if (string.IsNullOrWhiteSpace(request.NuevoNombreUsuario))
            return Result.Fallo("El nombre de usuario no puede estar vacío.", ResultErrorTipo.Validacion);

        var yaExiste = await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(request.NuevoNombreUsuario, usuarioId, cancellationToken);
        if (yaExiste)
            return Result.Fallo("Ese nombre de usuario ya está en uso.", ResultErrorTipo.Conflicto);

        usuario.NombreUsuario = request.NuevoNombreUsuario;
        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }
}
