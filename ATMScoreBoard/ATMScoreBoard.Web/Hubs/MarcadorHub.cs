using Microsoft.AspNetCore.SignalR;

namespace ATMScoreBoard.Web.Hubs
{
    public class MarcadorHub : Hub
    {
        public async Task JoinMesaGroup(string mesaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Mesa_{mesaId}");
        }
    }
}