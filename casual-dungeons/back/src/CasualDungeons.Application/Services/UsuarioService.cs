using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Usuarios;
using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync(cancellationToken);
        return usuarios.Select(MapearADto).ToList();
    }

    public async Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
            return Result<UsuarioDto>.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        return Result<UsuarioDto>.Exito(MapearADto(usuario));
    }

    public async Task<Result<UsuarioDto>> CrearAsync(CrearUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(dto.NombreUsuario, cancellationToken: cancellationToken))
            return Result<UsuarioDto>.Fallo("Ese nombre de usuario ya está en uso.", ResultErrorTipo.Conflicto);

        if (await _unitOfWork.Usuarios.ExisteEmailAsync(dto.Email, cancellationToken: cancellationToken))
            return Result<UsuarioDto>.Fallo("Ese correo electrónico ya está registrado.", ResultErrorTipo.Conflicto);

        var rol = await _unitOfWork.Roles.GetByNombreAsync(dto.Rol, cancellationToken);
        if (rol is null)
            return Result<UsuarioDto>.Fallo($"El rol '{dto.Rol}' no existe.", ResultErrorTipo.Validacion);

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            return Result<UsuarioDto>.Fallo("La contraseña debe tener al menos 8 caracteres.", ResultErrorTipo.Validacion);

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            RolId = rol.Id,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Configuración por defecto (modo oscuro activado).
        var configuracion = new ConfiguracionUsuario
        {
            UsuarioId = usuario.Id,
            TemaOscuro = true,
            FechaActualizacion = DateTime.UtcNow
        };
        await _unitOfWork.ConfiguracionesUsuario.AddAsync(configuracion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        usuario.Rol = rol;
        return Result<UsuarioDto>.Exito(MapearADto(usuario));
    }

    public async Task<Result<UsuarioDto>> EditarAsync(int id, EditarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
            return Result<UsuarioDto>.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        if (await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(dto.NombreUsuario, id, cancellationToken))
            return Result<UsuarioDto>.Fallo("Ese nombre de usuario ya está en uso.", ResultErrorTipo.Conflicto);

        if (await _unitOfWork.Usuarios.ExisteEmailAsync(dto.Email, id, cancellationToken))
            return Result<UsuarioDto>.Fallo("Ese correo electrónico ya está registrado.", ResultErrorTipo.Conflicto);

        var rol = await _unitOfWork.Roles.GetByNombreAsync(dto.Rol, cancellationToken);
        if (rol is null)
            return Result<UsuarioDto>.Fallo($"El rol '{dto.Rol}' no existe.", ResultErrorTipo.Validacion);

        if (!string.IsNullOrWhiteSpace(dto.NuevaPassword))
        {
            if (dto.NuevaPassword.Length < 8)
                return Result<UsuarioDto>.Fallo("La contraseña debe tener al menos 8 caracteres.", ResultErrorTipo.Validacion);

            usuario.PasswordHash = _passwordHasher.Hash(dto.NuevaPassword);
        }

        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.Email = dto.Email;
        usuario.RolId = rol.Id;
        usuario.Activo = dto.Activo;

        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        usuario.Rol = rol;
        return Result<UsuarioDto>.Exito(MapearADto(usuario));
    }

    public async Task<Result> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
            return Result.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        // No se elimina físicamente: se desactiva, ya que el usuario
        // puede tener campañas, personajes o participaciones asociadas
        // (las FK relevantes son Restrict, por lo que un borrado físico
        // fallaría si existen registros dependientes).
        usuario.Activo = false;
        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    public async Task<Result<ConfiguracionUsuarioDto>> GetConfiguracionAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdWithConfiguracionAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Result<ConfiguracionUsuarioDto>.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        var temaOscuro = usuario.Configuracion?.TemaOscuro ?? true;
        return Result<ConfiguracionUsuarioDto>.Exito(new ConfiguracionUsuarioDto(temaOscuro));
    }

    public async Task<Result<ConfiguracionUsuarioDto>> ActualizarConfiguracionAsync(int usuarioId, ConfiguracionUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdWithConfiguracionAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Result<ConfiguracionUsuarioDto>.Fallo("Usuario no encontrado.", ResultErrorTipo.NoEncontrado);

        if (usuario.Configuracion is null)
        {
            var nuevaConfiguracion = new ConfiguracionUsuario
            {
                UsuarioId = usuarioId,
                TemaOscuro = dto.TemaOscuro,
                FechaActualizacion = DateTime.UtcNow
            };
            await _unitOfWork.ConfiguracionesUsuario.AddAsync(nuevaConfiguracion, cancellationToken);
        }
        else
        {
            usuario.Configuracion.TemaOscuro = dto.TemaOscuro;
            _unitOfWork.ConfiguracionesUsuario.Update(usuario.Configuracion);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ConfiguracionUsuarioDto>.Exito(dto);
    }

    public async Task<List<UsuarioBasicoDto>> GetJugadoresAsync(CancellationToken cancellationToken = default)
    {
        var jugadores = await _unitOfWork.Usuarios.GetByRolNombreAsync("Jugador", cancellationToken);
        return jugadores.Select(j => new UsuarioBasicoDto(j.Id, j.NombreUsuario)).ToList();
    }

    private static UsuarioDto MapearADto(Usuario usuario) => new(
        usuario.Id,
        usuario.NombreUsuario,
        usuario.Email,
        usuario.Rol.Nombre,
        usuario.FechaCreacion,
        usuario.Activo,
        usuario.UltimoAcceso);
}
