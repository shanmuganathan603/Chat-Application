using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Hubs
{
    public class SignalingHub : Hub
    {
        private static Dictionary<string, string> Usernames = new Dictionary<string, string>();
        private static List<string> WaitingUsers = new List<string>();  // List of users waiting for pairing
        private static Dictionary<string, string> PairedUsers = new Dictionary<string, string>(); // Store pairs of users

        public override Task OnConnectedAsync()
        {
            Usernames[Context.User.Identity.Name] = Context.ConnectionId;
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User.Identity.Name;
            Usernames.Remove(userName);
            if (PairedUsers.ContainsKey(userName))
            {
                var pairedUser = PairedUsers[userName];
                PairedUsers.Remove(userName);
                PairedUsers.Remove(pairedUser);
                await Clients.Client(Usernames[pairedUser]).SendAsync("DisconnectedRandomChat");
                await Clients.Client(Usernames[pairedUser]).SendAsync("ReceiveMessage", $"{userName} has disconnected.");
            }
            else
            {
                WaitingUsers.Remove(userName);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(Usernames[user]).SendAsync("ReceiveMessage", message);
        }

        public async Task ConnectRandomUser()
        {
            var userName = Context.User.Identity.Name;
            var randomUser = WaitingUsers.FirstOrDefault(u => u != userName);
            if (!string.IsNullOrEmpty(randomUser))
            {
                WaitingUsers.Remove(randomUser);
                PairedUsers.Add(userName, randomUser);
                PairedUsers.Add(randomUser, userName);
                await Clients.Client(Usernames[userName]).SendAsync("ConnectedRandomChat", randomUser);
                await Clients.Client(Usernames[randomUser]).SendAsync("ConnectedRandomChat", userName);
            }
            else if (!WaitingUsers.Contains(userName))
            {
                WaitingUsers.Add(userName);
                await Clients.Client(Usernames[userName]).SendAsync("ReceiveMessage", "You are now waiting for a partner.");
            }
        }

        public async Task DisconnectChat()
        {
            var userName = Context.User.Identity.Name;
            if (PairedUsers.ContainsKey(userName))
            {
                var pairedUser = PairedUsers[userName];
                PairedUsers.Remove(userName);
                PairedUsers.Remove(pairedUser);
                await Clients.Client(Usernames[pairedUser]).SendAsync("DisconnectedRandomChat");
                await Clients.Client(Usernames[pairedUser]).SendAsync("ReceiveMessage", $"{userName} has disconnected.");
            }
            else
            {
                WaitingUsers.Add(userName);
            }
        }
    }
}