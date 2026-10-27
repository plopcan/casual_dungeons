using CasualDungeons.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CasualDungeons.Infrastructure.Realtime;

/// <summary>
/// Implementación de ICampaignNotifier sobre SignalR. Envía los
/// eventos al grupo "campania-{campaniaId}" del CampaignHub.
/// </summary>
public class CampaignNotifier : ICampaignNotifier
{
    private readonly IHubContext<CampaignHub> _hubContext;

    public CampaignNotifier(IHubContext<CampaignHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotificarPersonajeActualizadoAsync(int campaniaId, int personajeId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"campania-{campaniaId}")
            .SendAsync("PersonajeActualizado", personajeId, cancellationToken);
    }

    public Task NotificarNPCActualizadoAsync(int campaniaId, int npcId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"campania-{campaniaId}")
            .SendAsync("NPCActualizado", npcId, cancellationToken);
    }

    public Task NotificarMonstruoCampaniaActualizadoAsync(int campaniaId, int monstruoCampaniaId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"campania-{campaniaId}")
            .SendAsync("MonstruoCampaniaActualizado", monstruoCampaniaId, cancellationToken);
    }

    public Task NotificarCombateActualizadoAsync(int campaniaId, int combateId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"campania-{campaniaId}")
            .SendAsync("CombateActualizado", combateId, cancellationToken);
    }
}
