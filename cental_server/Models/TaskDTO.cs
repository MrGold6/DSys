namespace cental_server.Models
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Calculation { get; set; }
        public string Result { get; set; }
        public TaskDTOStatus Status { get; set; }
        public int Client_id { get; set; }
    }
}
