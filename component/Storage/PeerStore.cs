using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MyHybridApp.Helper;
using MyHybridApp.Models;

namespace MyHybridApp.Storage
{
    public static class PeerStore
    {
        private static readonly string FilePath = "peers.json";
        private static readonly ConcurrentDictionary<string, DateTime> _peers = new();

        public static IReadOnlyCollection<string> All => _peers.Keys.ToList();

        public static string OwnAddress { get; set; }
        public static string ClientId { get; set; }

        public static string CurrentCenter { get; set; }

        private static bool _manualOverride = false;
        private static DateTime _lastManualSet = DateTime.MinValue;
        public static bool IsDistributingTasks = false;


        public static void SetAsCenter(string address)
        {
            CurrentCenter = address;
            Logger.Log($"The center is intended to: {address}");
        }


        public static bool IsCenter() => OwnAddress == CurrentCenter;

        public static bool IsCenter(string url) => url == CurrentCenter;

        public static bool IsCenterMe(string url, string ownAddress) => ownAddress == url;

        public static void SetManualOverride(bool value)
        {
            _manualOverride = value;
            if (value) _lastManualSet = DateTime.UtcNow;
        }

        public static bool IsManualOverride() => _manualOverride;

        public static bool IsCooldownActive() =>
            _manualOverride && (DateTime.UtcNow - _lastManualSet).TotalSeconds < 30;

        public static void ResetManualOverride()
        {
            if ((DateTime.UtcNow - _lastManualSet).TotalSeconds >= 30)
                _manualOverride = false;
        }

        public static void InitWithInitialPeer(string initialPeer)
        {
            _peers.Clear();
            _peers[initialPeer] = DateTime.UtcNow;
            Save();
        }

        public static void Announce(string address)
        {
            _peers[address] = DateTime.UtcNow;
            Save();
        }

        public static List<string> GetOtherPeers(string ownAddress)
        {
            return _peers.Keys
                .Where(p => !p.Equals(ownAddress, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<string> GetPeers()
        {
            return _peers.Keys.ToList();
        }

        public static void Load()
        {
            if (!File.Exists(FilePath)) return;
            try
            {
                var json = File.ReadAllText(FilePath);
                var peerFile = JsonSerializer.Deserialize<PeerFileModel>(json);

                var list = peerFile.Nodes;

                if (list != null)
                {
                    foreach (var addr in list)
                        _peers[addr] = DateTime.UtcNow;
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                PeerFileModel model = new PeerFileModel();
                model.Nodes = _peers.Keys.Distinct().ToList();
                model.CurrentCenter = CurrentCenter;

                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(FilePath, json);
            }
            catch { }
        }

        public static void MergePeers(IEnumerable<string> addresses)
        {
            foreach (var addr in addresses)
                _peers[addr] = DateTime.UtcNow;

            Save();
        }

        public static void Remove(string address)
        {
            _peers.TryRemove(address, out _);
            Save();
        }

    }

}
