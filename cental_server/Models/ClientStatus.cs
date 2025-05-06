namespace cental_server.Models
{
    public class ClientStatus
    {
        public string Sender { get; set; }
        public double CpuUsage { get; set; }     // %
        public double MemoryUsage { get; set; }  // МБ
    }

}
