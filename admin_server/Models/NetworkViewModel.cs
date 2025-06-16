namespace cental_server.Models
{
    public class NetworkViewModel
    {
        public List<string> Peers { get; set; }
        //todo sort to do center first
        public List<ClientStatus> Statuses { get; set; }
        public string CurrentCenter { get; set; }
        public List<MyTask> Tasks { get; set; }

    }
}
