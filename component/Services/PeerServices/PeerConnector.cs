using client_wpf_app.Storage;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using MyHybridApp.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;

namespace MyHybridApp.Services.PeerServices
{
    public static class PeerConnector
    {
        public static readonly ConcurrentDictionary<string, HubConnection> _connections = new();

        //connection section
        public static async Task ConnectToPeersAsync(string ownAddress)
        {
            var peers = PeerStore.GetOtherPeers(ownAddress);

            foreach (var peer in peers)
            {
                await ConnectToPeerAsync(peer, ownAddress);
            }
        }

        private static async Task ConnectToPeerAsync(string peer, string ownAddress)
        {
            if (_connections.ContainsKey(peer))
            {
                return;
            }

            if (_connections.TryGetValue(peer, out var existing))
            {
                if (existing.State == HubConnectionState.Connected ||
                    existing.State == HubConnectionState.Connecting)
                {
                    Logger.Log($"Connection {peer} is existing in state {existing.State}.");

                    return;
                }

                try
                {
                    await existing.StartAsync();
                    Logger.Log($"Connection is renewed {peer}");

                    return;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error connecting to {peer}: {ex.Message}");
                    _connections.TryRemove(peer, out _);
                }
            }

            var conn = new HubConnectionBuilder()
                .WithUrl($"{peer}/peerhub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                })
                .WithAutomaticReconnect()
                .Build();

            try
            {
                Logger.Log($"Status before connecting to the {peer}: {conn.State}");

                if (conn.State == HubConnectionState.Disconnected)
                {
                    await conn.StartAsync();
                    _connections[peer] = conn;

                    await conn.InvokeAsync("AnnouncePeer", ownAddress);

                    Logger.Log($"Successfully connected to {peer}");
                }
                else
                {
                    Logger.Log($"Cannot connect {peer}, state: {conn.State}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error connecting to {peer}: {ex.Message}");
            }
        }

       //peer managment section
        public static async Task SendStatusToAllPeers(string ownAddress, object status)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value.State != HubConnectionState.Connected)
                {
                    Logger.Log($"Skip sending status to {kvp.Key} — inactive connection ({kvp.Value.State})");
                    continue;
                }

                if (kvp.Key == ownAddress)
                {
                    Logger.Log($"Skip sending status to {kvp.Key} myself");
                    continue;
                }

                try
                {
                    await kvp.Value.InvokeAsync("ReceivePeerStatus", status);

                }
                catch (Exception ex)
                {
                    Logger.Log($"Unable to send status {kvp.Key}: {ex.Message}");
                }
            }
        }

        public static async Task RequestPeersAndMergeAsync()
        {
            foreach (var kvp in _connections)
            {
                try
                {
                    var peers = await kvp.Value.InvokeAsync<List<string>>("GetPeers");
                    PeerStore.MergePeers(peers);

                    PeerStore.Save();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not get peers from {kvp.Key}: {ex.Message}");
                }
            }
        }
        public static void CleanupRemovedPeers(string ownAddress)
        {
            var currentPeers = PeerStore.GetOtherPeers(ownAddress);
            var activePeers = _connections.Keys.ToList();

            foreach (var peer in activePeers)
            {
                if (!currentPeers.Contains(peer))
                {
                    if (_connections.TryRemove(peer, out var conn))
                    {
                        Logger.Log($"Close the connection to the remote node - inactive connection {peer}");
                        _ = conn.StopAsync(); // не чекаємо
                        conn.DisposeAsync();  // очищення ресурсів
                    }
                }
            }
        }


        //center section
        public static async Task RequestCenterFromPeer()
        {
            foreach (var kvp in _connections)
            {
                try
                {
                    var center = await kvp.Value.InvokeAsync<string>("GetCenter");
                    Logger.Log($"The center is received {center}");
                    PeerStore.SetAsCenter(center);
                    PeerStore.Save();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not get peers from {kvp.Key}: {ex.Message}");
                }
            }
        }
     
        
        public static async Task BroadcastNewCenterAsync(string newCenter, bool isManual)
        {
            foreach (var kvp in _connections)
            {

                if (kvp.Value.State != HubConnectionState.Connected)
                {
                    Logger.Log($"Skip sending the center to {kvp.Key} - inactive connection ({kvp.Value.State})");
                    continue;
                }

                if (kvp.Key == PeerStore.OwnAddress)
                {
                    Logger.Log($"Skip sending the center to {kvp.Key} myself");
                    continue;
                }

                try
                {
                    await kvp.Value.InvokeAsync("ReceiveNewCenter", newCenter, isManual);

                }

                catch (Exception ex)
                {
                    Logger.Log($"Could not send center {kvp.Key}: {ex.Message}");
                }
            }
        }


        //task section

        public static async Task RequestTasksFromPeer()
        {
            foreach (var kvp in _connections)
            {
                try
                {
                    var tasks = await kvp.Value.InvokeAsync<List<MyTask>>("GetAllTasks");
                    Logger.Log($"Got list of task");

                    foreach (var task in tasks)
                    {
                        await TaskRepository.AddOrUpdateTask(task);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not get peers from {kvp.Key}: {ex.Message}");
                }
            }
        }

        public static async Task RequestResultsFromPeer()
        {
            foreach (var kvp in _connections)
            {
                try
                {
                    var tasks = await kvp.Value.InvokeAsync<List<TaskExpression>>("GetAllResults");
                    Logger.Log($"Got list of task");

                    foreach (var task in tasks)
                    {
                        ResultRepository.SaveResult(task);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not get peers from {kvp.Key}: {ex.Message}");
                }
            }
        }

        public static async Task SendTaskTo(string peerAddress, TaskExpression task)
        {
            if (_connections.TryGetValue(peerAddress, out var conn))
            {
                try
                {
                    await conn.InvokeAsync("ReceiveTask", task);
                    Logger.Log($"Tasks for {peerAddress}: {task.Expression}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not send a task to {peerAddress}: {ex.Message}");
                }
            }
        }

        public static async Task SendTaskFromAdminAllPeers(MyTask task)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value.State != HubConnectionState.Connected)
                {
                    Logger.Log($"Skip sending a task to {kvp.Key} - inactive connection ({kvp.Value.State})");
                    continue;
                }

                if (kvp.Key == PeerStore.OwnAddress)
                {
                    Logger.Log($"Skip sending a task to {kvp.Key} myself");
                    continue;
                }

                try
                {
                    await kvp.Value.InvokeAsync("SaveTaskFromAdmin", task);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Task submission failed {kvp.Key}: {ex.Message}");
                }
            }

        }

        public static async Task SendResultToAllPeers(TaskExpression result)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value.State != HubConnectionState.Connected)
                {
                    Logger.Log($" Skip sending the result to {kvp.Key} - inactive connection ({kvp.Value.State})");
                    continue;
                }

                if (kvp.Key == PeerStore.OwnAddress)
                {
                    Logger.Log($" Skip sending the result to {kvp.Key} myself");
                    continue;
                }

                try
                {
                    await kvp.Value.InvokeAsync("ReceiveResult", result);
                }
                catch (Exception ex)
                {
                    Logger.Log($"The result could not be sent {kvp.Key}: {ex.Message}");
                }
            }

        }

        public static async Task SendStatusOfTaskToOtherPeers(MyTask task)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value.State != HubConnectionState.Connected)
                {
                    Logger.Log($"Skip sending task status to{kvp.Key} - inactive connection ({kvp.Value.State})");
                    continue;
                }

                if (kvp.Key == PeerStore.OwnAddress)
                {
                    Logger.Log($"Skip sending task status to{kvp.Key} myself");
                    continue;
                }

                try
                {
                    Logger.Log($"Sending task status to {kvp.Key} {task.Status}");

                    await kvp.Value.InvokeAsync("ReceiveTaskUpdate", task);

                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to send task status {kvp.Key}: {ex.Message}");
                }
            }

        }
        public static async Task DeleteAllResultsByTaskIdToOtherPeers(string TaskId)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value.State != HubConnectionState.Connected)
                {
                    Logger.Log($"Skip DeleteAllResultsByTaskIdToOtherPeers до {kvp.Key} - inactive connection ({kvp.Value.State})");
                    continue;
                }

                if (kvp.Key == PeerStore.OwnAddress)
                {
                    Logger.Log($"Skip DeleteAllResultsByTaskIdToOtherPeers to {kvp.Key} myself");
                    continue;
                }

                try
                {
                    Logger.Log($"DeleteAllResultsByTaskIdToOtherPeers {kvp.Key} {TaskId}");

                    await kvp.Value.InvokeAsync("DeleteResultByTaskId", TaskId);

                }
                catch (Exception ex)
                {
                    Logger.Log($"Can't do DeleteAllResultsByTaskIdToOtherPeers {kvp.Key}: {ex.Message}");
                }
            }

        }


        //Disconection section
        public static async Task SendManualDisconnectTo(string peerAddress, string clientId)
        {
            if (_connections.TryGetValue(peerAddress, out var conn))
            {
                try
                {
                    await conn.InvokeAsync("RemoteDisconnect", peerAddress, clientId);
                    Logger.Log($"RemoteDisconnect to {peerAddress}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Can't do RemoteDisconnect {peerAddress}: {ex.Message}");
                }
            }
        }

        public static async Task BroadcastGoodbyeAsync(string ownAddress, string clientId)
        {
            foreach (var kvp in _connections)
            {
                try
                {
                    await kvp.Value.InvokeAsync("Goodbye", ownAddress, clientId);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error with goodbye to {kvp.Key}: {ex.Message}");
                }
            }

            ClientStatusStore.DeleteAll();
        }


        public static void StopAll()
        {
            foreach (var conn in _connections.Values)
            {
                conn.StopAsync();
            }
            _connections.Clear();
        }

    }
}

