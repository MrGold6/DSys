using Microsoft.AspNetCore.SignalR;

namespace cental_server.Services
{
    public class ConnectionHub : Hub
    {

        //public override Task OnConnectedAsync()
        //{
        //    // Отримуємо ім'я користувача з параметрів запиту
        //    var username = Context.GetHttpContext()?.Request.Query["username"];

        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        ClientStorage._userConnections[username] = Context.ConnectionId;
        //    }

        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception exception)
        //{
        //    // Видаляємо користувача зі словника при відключенні
        //    var username = ClientStorage._userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        ClientStorage._userConnections.TryRemove(username, out _);
        //        ClientStorage._clientStatus.TryRemove(username, out _);
        //    }

        //    return base.OnDisconnectedAsync(exception);
        //}

        //public static string? GetConnectionIdByUsername(string username)
        //{
        //    return ClientStorage._userConnections.TryGetValue(username, out var connId) ? connId : null;
        //}



        //public async Task SendToServer(MessageWrapper<String> message)
        //{
        //    Console.WriteLine($" Отримано від {message.Sender}: {message.Response}");

        //    MessageStore.Add(message); // ЗБЕРЕЖЕННЯ

        //    // Розсилка всім клієнтам
        //    await Clients.All.SendAsync("ReceiveMessage", message);
        //}

        //public Task ReportStatus(MessageWrapper<ClientStatus> message)
        //{
        //    //todo rebild this logic to smth cleaner
        //    ClientStatus clientStatus = message.Response;
        //    clientStatus.Sender = message.Sender;
        //    ClientStorage._clientStatus[message.Sender] = clientStatus;
        //    Console.WriteLine($"{message.Sender} | CPU: {message.Response.CpuUsage}% | RAM: {message.Response.MemoryUsage}MB");
        //    return Task.CompletedTask;
        //}

    }
}

