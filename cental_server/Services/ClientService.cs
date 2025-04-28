using cental_server.Models;
using cental_server.Storages;

namespace cental_server.Services
{
    public class ClientService
    {
        public static IReadOnlyDictionary<string, ClientStatus> GetAllStatuses()
        {
            return ClientStorage._clientStatus;
        }

        public static int GetCountOfClients()
        {
            return ClientStorage._userConnections.Count;
        }

        public static ClientStatus GetStatusByUser(string clientId)
        {
            return ClientStorage._clientStatus[clientId];
        }
    }
}
