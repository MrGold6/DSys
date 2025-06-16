using System;

namespace client_wpf_app.Models
{
    public class ClientStatus
    {
        public string ClientId { get; set; }
        public string ClientUrl { get; set; }
        public double CpuUsage { get; set; }  
        public double MemoryUsage { get; set; }
        public bool IsCenter { get; set; }
        public DateTime Timestamp { get; set; }
        public double TotalMemoryMB { get; set; }
        public int MemoryUsagePercent => (int)((MemoryUsage / TotalMemoryMB) * 100);

        public ClientStatus() { }

    }
}
