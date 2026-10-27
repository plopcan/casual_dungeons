using CasualDungeons.Application.Common;
using CasualDungeons.Application.DTOs.Monstruos;
using CasualDungeons.Application.DTOs.NPCs;

namespace CasualDungeons.Application.Interfaces;

public interface INPCService
{
    Task<Result<List<NPCDto>>> GetByCampaniaAsync(int campaniaId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<NPCDto>> GetByIdAsync(int npcId, int dungeonMasterId, CancellationToken cancellationToken = default);

    Task<Result<NPCDto>> CrearAsync(int campaniaId, int dungeonMasterId, CrearNPCDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un NPC en la campaña copiando el stat block de un
    /// monstruo del catálogo precreado. Los rasgos y acciones del
    /// monstruo (que el NPC no modela como campos propios) se anexan
    /// como texto a las Notas del NPC para no perder esa información.
    /// </summary>
    Task<Result<NPCDto>> CrearDesdeCatalogoAsync(int campaniaId, int dungeonMasterId, CrearNpcDesdeCatalogoDto dto, CancellationToken cancellationToken = default);

    Task<Result<NPCDto>> EditarAsync(int npcId, int dungeonMasterId, EditarNPCDto dto, CancellationToken cancellationToken = default);

    Task<Result> EliminarAsync(int npcId, int dungeonMasterId, CancellationToken cancellationToken = default);
}
