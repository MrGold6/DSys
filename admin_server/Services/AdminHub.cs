using cental_server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace cental_server.Services
{
    public class AdminHub
    {
        private readonly HubConnection _connection;

        public bool IsStarted => _connection.State == HubConnectionState.Connected;

        public AdminHub(string nodeAddress)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{nodeAddress}/peerhub")
                .WithAutomaticReconnect()
                .Build();

            _connection.Closed += async (error) =>
            {
                Console.WriteLine("❌ SignalR зʼєднання втрачено: " + error?.Message);
                await Task.Delay(2000);
                await _connection.StartAsync();
            };

            _connection.Reconnected += connectionId =>
            {
                Console.WriteLine("🔄 SignalR відновлено: " + connectionId);
                return Task.CompletedTask;
            };

            _connection.Reconnecting += error =>
            {
                Console.WriteLine("⏳ SignalR перезапускається...");
                return Task.CompletedTask;
            };


        }

        public async Task StartAsync() => await _connection.StartAsync();
        public async Task StopAsync() => await _connection.StopAsync();
        public async Task SendTask(MyTask task)
        {
            await _connection.InvokeAsync("ReceiveAdminTask", task);
        }

        public async Task SetCenter(string newCenter)
        {
            await _connection.InvokeAsync("ReceiveManualyCenter", newCenter);
        }


        public async Task DisconnectPeer(string newCenter, string clientId)
        {
            await _connection.InvokeAsync("ManualDisconnectPeer", newCenter, clientId);
        }



        public async Task<List<ClientStatus>> FetchStatusesAsync()
        {
            return await _connection.InvokeAsync<List<ClientStatus>>("GetStatuses");
        }

        public async Task<List<MyTask>> FetchTasksAsync()
        {
            return await _connection.InvokeAsync<List<MyTask>>("GetTasks");
        }

        public async Task<List<TaskExpression>> FetchTaskExpressionAsync(string taskId)
        {
            return await _connection.InvokeAsync<List<TaskExpression>>("GetTaskExpressionAdmin", taskId);
        }


        public async Task<MyTask> FetchTaskByIdAsync(string taskId)
        {
            return await _connection.InvokeAsync<MyTask>("GetTaskByIdAdmin", taskId);
        }

        public async Task<List<string>> FetchPeersAsync()
        {
            return await _connection.InvokeAsync<List<string>>("GetPeers");
        }

        public async Task<string> FetchCenterAsync()
        {
            return await _connection.InvokeAsync<string>("GetCenter");
        }


    }

}
