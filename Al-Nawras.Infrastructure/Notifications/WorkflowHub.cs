using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Al_Nawras.Infrastructure.Notifications
{
    public class WorkflowHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, WorkflowHubGroupNames.User(userId));

            var roleClaims = Context.User?.FindAll(ClaimTypes.Role).Select(x => x.Value) ?? [];
            foreach (var role in roleClaims.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
                await Groups.AddToGroupAsync(Context.ConnectionId, WorkflowHubGroupNames.Role(role));

            var clientId = Context.User?.FindFirstValue("clientId");
            if (!string.IsNullOrWhiteSpace(clientId))
                await Groups.AddToGroupAsync(Context.ConnectionId, WorkflowHubGroupNames.Client(clientId));

            await base.OnConnectedAsync();
        }
    }
}
