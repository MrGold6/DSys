using cental_server.Models;

namespace cental_server.Services
{
    public class TaskService
    {
        List<TaskDTO> tasks = new List<TaskDTO>();
        public TaskService() { }

        public List<TaskDTO> taskLoader()
        {
            //todo
            return null;
        }
        public void AddTask(TaskDTO task)
        {
            //todo
        }

        public void RemoveTask(TaskDTO task) { }
        public TaskDTO GetTask(int id)
        {
            //todo
            return null;
        }
        public List<TaskDTO> GetToDoTasks()
        {
            return tasks.Where(t => t.Status == TaskDTOStatus.TODO).ToList();
        }

        public List<TaskDTO> GetInProgressTasks()
        {
            return tasks.Where(t => t.Status == TaskDTOStatus.PROGRESS).ToList();
        }

        public List<TaskDTO> GetCompletedTasks()
        {
            return tasks.Where(t => t.Status == TaskDTOStatus.DONE).ToList();
        }

        public Dictionary<string, string> GetCalculationAndResults()
        {
            return tasks.ToDictionary(t => t.Calculation, t => t.Result);
        }
    }
}
