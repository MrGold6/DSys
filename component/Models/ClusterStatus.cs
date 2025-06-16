using client_wpf_app.Models;
using System.Collections.Generic;

namespace MyHybridApp.Models
{
    public class ClusterStatus
    {
        public string CenterId { get; set; } = "";
        public List<ClientStatus> Nodes { get; set; } = new();
    }

}
