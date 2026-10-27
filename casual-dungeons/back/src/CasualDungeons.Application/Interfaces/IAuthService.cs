using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Auth;

namespace CasualDungeons.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task<Result> CambiarPasswordAsync(int usuarioId, CambiarPasswordRequestDto request, CancellationToken cancellationToken = default);

    Task<Result> CambiarNombreUsuarioAsync(int usuarioId, CambiarNombreUsuarioRequestDto request, CancellationToken cancellationToken = default);
}
