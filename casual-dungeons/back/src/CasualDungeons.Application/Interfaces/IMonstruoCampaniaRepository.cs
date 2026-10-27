using CasualDungeons.Domain.Entities;

namespace CasualDungeons.Application.Interfaces;

public interface IMonstruoCampaniaRepository : IRepository<MonstruoCampania>
{
    Task<List<MonstruoCampania>> GetByCampaniaAsync(int campaniaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina, si existen, las filas de ParticipantesCombate que
    /// referencian a este monstruo (de combates actuales o ya
    /// pasados). Necesario antes de borrar el monstruo: la FK
    /// correspondiente es Restrict, igual que para NPC.
    /// </summary>
    Task EliminarParticipacionesEnCombateAsync(int monstruoCampaniaId, CancellationToken cancellationToken = default);
}
