using cental_server.Models;
using System.Collections.Concurrent;

namespace cental_server.Storages
{
    public class ClientStorage
    {
        // Потокобезпечний словник для зберігання відповідностей між іменами користувачів та ConnectionId
        public static ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        public static readonly ConcurrentDictionary<string, ClientStatus> _clientStatus = new();
    }
}
