using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Hubs
{
    public class SignalingHub : Hub
    {
        private static Dictionary<string, string> Usernames = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            Usernames.Add(Context.ConnectionId, Context.User.Identity.Name);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Usernames.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        public async Task GetUsernames()
        {
            
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
