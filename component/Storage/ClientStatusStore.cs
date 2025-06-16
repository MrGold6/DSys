using client_wpf_app.Models;
using MyHybridApp.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace client_wpf_app.Storage
{
    public static class ClientStatusStore
    {

        public static ConcurrentDictionary<string, ClientStatus> Statuses = new ConcurrentDictionary<string, ClientStatus>();
        public static string CurrentCenter { get; set; } = "";

        private static readonly ConcurrentDictionary<string, DateTime> _connectedAt = new();


        public static void Add(ClientStatus status)
        {
            Statuses[status.ClientId] = status;

            if (!_connectedAt.ContainsKey(status.ClientId))
                _connectedAt[status.ClientId] = DateTime.UtcNow;
        }

        public static bool IsNewClient(string clientId)
        {
            if (_connectedAt.TryGetValue(clientId, out var time))
            {
                return (DateTime.UtcNow - time) < TimeSpan.FromSeconds(15);
            }
            return true;
        }


        public static ClientStatus GetLeastLoaded()
        {
            return Statuses
                .Where(x => x.Value.ClientUrl != CurrentCenter && !IsNewClient(x.Value.ClientId))
                .OrderBy(x => x.Value.CpuUsage + x.Value.MemoryUsage)
                .FirstOrDefault().Value;
        }

        public static void Remove(string clientId)
        {
            Statuses.TryRemove(clientId, out _);
        }

        //todo why sometimes returns with center???
        public static List<ClientStatus> GetOptimalClients()
        {
            return Statuses.Values
                .Where(c => c.CpuUsage < 80 && c.MemoryUsage < 1000 && c.ClientUrl != CurrentCenter) //1 GB
                .OrderBy(c => c.CpuUsage)
                .ToList();
        }

        public static List<ClientStatus> GetAll() => Statuses.Values.ToList();
        public static void DeleteAll() => Statuses.Clear();

    }

}
