namespace CasualDungeons.Application.Interfaces;

/// <summary>
/// Abstracción sobre el mecanismo de notificación en tiempo real
/// (SignalR). Permite que los servicios de Application notifiquen
/// cambios sin depender directamente de SignalR ni de ASP.NET Core.
///
/// Los grupos de SignalR se organizan por campaña: todos los
/// clientes conectados a una campaña (DM y jugadores) se unen al
/// grupo "campania-{campaniaId}".
/// </summary>
public interface ICampaignNotifier
{
    /// <summary>
    /// Notifica que la ficha de un personaje fue actualizada.
    /// El DM conectado a la campaña recibe el evento sin recargar.
    /// </summary>
    Task NotificarPersonajeActualizadoAsync(int campaniaId, int personajeId, CancellationToken cancellationToken = default);

    Task NotificarNPCActualizadoAsync(int campaniaId, int npcId, CancellationToken cancellationToken = default);

    Task NotificarMonstruoCampaniaActualizadoAsync(int campaniaId, int monstruoCampaniaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica cambios en el orden de iniciativa o estado de un combate.
    /// </summary>
    Task NotificarCombateActualizadoAsync(int campaniaId, int combateId, CancellationToken cancellationToken = default);
}
