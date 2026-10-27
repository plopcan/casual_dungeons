using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Campanias;
using CasualDungeons.Application.Interfaces;
using CasualDungeons.Domain.Entities;
using CasualDungeons.Domain.Enums;

namespace CasualDungeons.Application.Services;

public class CampaniaService : ICampaniaService
{
    private readonly IUnitOfWork _unitOfWork;

    public CampaniaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CampaniaResumenDto>> GetMisCampaniasAsync(int usuarioId, bool activas, CancellationToken cancellationToken = default)
    {
        var campanias = await _unitOfWork.Campanias.GetByUsuarioParticipanteAsync(usuarioId, cancellationToken);

        var estadoFiltro = activas ? EstadoCampania.Activa : EstadoCampania.Finalizada;

        return campanias
            .Where(c => c.Estado == estadoFiltro)
            .Select(c => new CampaniaResumenDto(
                c.Id,
                c.Nombre,
                c.Descripcion,
                c.ImagenUrl,
                c.FechaInicio,
                c.FechaFinalizacion,
                c.Estado.ToString(),
                c.DungeonMasterId == usuarioId))
            .ToList();
    }

    public async Task<Result<CampaniaDetalleDto>> GetDetalleAsync(int campaniaId, int usuarioSolicitanteId, CancellationToken cancellationToken = default)
    {
        var campania = await _unitOfWork.Campanias.GetByIdWithParticipantesAsync(campaniaId, cancellationToken);
        if (campania is null)
            return Result<CampaniaDetalleDto>.Fallo("Campaña no encontrada.", ResultErrorTipo.NoEncontrado);

        var esDM = campania.DungeonMasterId == usuarioSolicitanteId;
        var esParticipante = campania.Participantes.Any(p => p.UsuarioId == usuarioSolicitanteId);

        if (!esDM && !esParticipante)
            return Result<CampaniaDetalleDto>.Fallo("No tienes acceso a esta campaña.", ResultErrorTipo.SinPermiso);

        return Result<CampaniaDetalleDto>.Exito(MapearADetalleDto(campania));
    }

    public async Task<Result<CampaniaDetalleDto>> CrearAsync(int dungeonMasterId, CrearCampaniaDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<CampaniaDetalleDto>.Fallo("El nombre de la campaña es obligatorio.", ResultErrorTipo.Validacion);

        var validacionJugadores = await ValidarJugadoresExistenAsync(dto.JugadoresIds, cancellationToken);
        if (!validacionJugadores.EsExitoso)
            return Result<CampaniaDetalleDto>.Fallo(validacionJugadores.Error!, validacionJugadores.TipoError!.Value);

        var campania = new Campania
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            ImagenUrl = dto.ImagenUrl,
            FechaInicio = dto.FechaInicio,
            Estado = EstadoCampania.Activa,
            DungeonMasterId = dungeonMasterId,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Campanias.AddAsync(campania, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var jugadorId in dto.JugadoresIds.Distinct())
        {
            await _unitOfWork.Campanias.AgregarParticipanteAsync(campania.Id, jugadorId, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var campaniaCompleta = await _unitOfWork.Campanias.GetByIdWithParticipantesAsync(campania.Id, cancellationToken);
        return Result<CampaniaDetalleDto>.Exito(MapearADetalleDto(campaniaCompleta!));
    }

    public async Task<Result<CampaniaDetalleDto>> EditarAsync(int campaniaId, int dungeonMasterId, EditarCampaniaDto dto, CancellationToken cancellationToken = default)
    {
        var campania = await _unitOfWork.Campanias.GetByIdWithParticipantesAsync(campaniaId, cancellationToken);
        if (campania is null)
            return Result<CampaniaDetalleDto>.Fallo("Campaña no encontrada.", ResultErrorTipo.NoEncontrado);

        if (campania.DungeonMasterId != dungeonMasterId)
            return Result<CampaniaDetalleDto>.Fallo("Solo el Dungeon Master propietario puede editar la campaña.", ResultErrorTipo.SinPermiso);

        if (!Enum.TryParse<EstadoCampania>(dto.Estado, out var nuevoEstado))
            return Result<CampaniaDetalleDto>.Fallo("Estado de campaña no válido.", ResultErrorTipo.Validacion);

        if (dto.FechaFinalizacion is not null && dto.FechaFinalizacion < dto.FechaInicio)
            return Result<CampaniaDetalleDto>.Fallo("La fecha de finalización no puede ser anterior a la de inicio.", ResultErrorTipo.Validacion);

        var validacionJugadores = await ValidarJugadoresExistenAsync(dto.JugadoresIds, cancellationToken);
        if (!validacionJugadores.EsExitoso)
            return Result<CampaniaDetalleDto>.Fallo(validacionJugadores.Error!, validacionJugadores.TipoError!.Value);

        campania.Nombre = dto.Nombre;
        campania.Descripcion = dto.Descripcion;
        campania.ImagenUrl = dto.ImagenUrl;
        campania.FechaInicio = dto.FechaInicio;
        campania.FechaFinalizacion = dto.FechaFinalizacion;
        campania.Estado = nuevoEstado;

        _unitOfWork.Campanias.Update(campania);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Sincronizar participantes: eliminar los que ya no están,
        // añadir los nuevos. Se compara contra el snapshot cargado
        // antes de modificar la campaña.
        var idsActuales = campania.Participantes.Select(p => p.UsuarioId).ToHashSet();
        var idsDeseados = dto.JugadoresIds.ToHashSet();

        foreach (var usuarioId in idsActuales.Except(idsDeseados))
        {
            await _unitOfWork.Campanias.EliminarParticipanteAsync(campaniaId, usuarioId, cancellationToken);
        }

        foreach (var usuarioId in idsDeseados.Except(idsActuales))
        {
            await _unitOfWork.Campanias.AgregarParticipanteAsync(campaniaId, usuarioId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var campaniaActualizada = await _unitOfWork.Campanias.GetByIdWithParticipantesAsync(campania.Id, cancellationToken);
        return Result<CampaniaDetalleDto>.Exito(MapearADetalleDto(campaniaActualizada!));
    }

    public async Task<Result> EliminarAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default)
    {
        var campania = await _unitOfWork.Campanias.GetByIdAsync(campaniaId, cancellationToken);
        if (campania is null)
            return Result.Fallo("Campaña no encontrada.", ResultErrorTipo.NoEncontrado);

        if (campania.DungeonMasterId != dungeonMasterId)
            return Result.Fallo("Solo el Dungeon Master propietario puede eliminar la campaña.", ResultErrorTipo.SinPermiso);

        // La cascada de base de datos (Campania -> Personaje, NPC,
        // Combate, ParticipanteCampania) elimina automáticamente
        // todos los datos dependientes.
        _unitOfWork.Campanias.Remove(campania);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Exito();
    }

    private async Task<Result> ValidarJugadoresExistenAsync(List<int> jugadoresIds, CancellationToken cancellationToken)
    {
        foreach (var jugadorId in jugadoresIds.Distinct())
        {
            var jugador = await _unitOfWork.Usuarios.GetByIdAsync(jugadorId, cancellationToken);
            if (jugador is null)
                return Result.Fallo($"El usuario con Id {jugadorId} no existe.", ResultErrorTipo.Validacion);
        }

        return Result.Exito();
    }

    private static CampaniaDetalleDto MapearADetalleDto(Campania campania) => new(
        campania.Id,
        campania.Nombre,
        campania.Descripcion,
        campania.ImagenUrl,
        campania.FechaInicio,
        campania.FechaFinalizacion,
        campania.Estado.ToString(),
        campania.DungeonMasterId,
        campania.DungeonMaster?.NombreUsuario ?? string.Empty,
        campania.Participantes.Select(p => new ParticipanteDto(
            p.UsuarioId,
            p.Usuario?.NombreUsuario ?? string.Empty,
            null,
            null)).ToList());
}
