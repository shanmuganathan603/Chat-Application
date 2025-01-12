using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Hubs
{
    public class SignalingHub : Hub
    {
        private static Dictionary<string, string> Usernames = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            Usernames.Add(Context.User.Identity.Name, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Usernames.Remove(Context.User.Identity.Name);
            return base.OnDisconnectedAsync(exception);
        }
        public async Task GetUsernames()
        {

        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(Usernames[user]).SendAsync("ReceiveMessage", message);
        }
    }
}