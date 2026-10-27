using Microsoft.AspNetCore.SignalR;

namespace CasualDungeons.Infrastructure.Realtime;

/// <summary>
/// Hub de SignalR para sincronización en tiempo real dentro de una
/// campaña. Los clientes (DM y jugadores) se unen al grupo
/// "campania-{campaniaId}" al conectar a la vista de una campaña,
/// y reciben los eventos definidos en ICampaignNotifier.
///
/// El cliente debe llamar al método UnirseACampania tras conectar,
/// pasando el Id de la campaña que está visualizando.
/// </summary>
public class CampaignHub : Hub
{
    private static string GrupoCampania(int campaniaId) => $"campania-{campaniaId}";

    public async Task UnirseACampania(int campaniaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoCampania(campaniaId));
    }

    public async Task SalirDeCampania(int campaniaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoCampania(campaniaId));
    }
}
