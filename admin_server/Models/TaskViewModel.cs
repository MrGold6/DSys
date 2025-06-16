namespace cental_server.Models
{
    public class TaskViewModel
    {
        public MyTask Task { get; set; }
        public List<TaskExpression> SubTask { get; set; }

    }
}
