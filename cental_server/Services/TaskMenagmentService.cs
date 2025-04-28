using cental_server.Models;

namespace cental_server.Services
{
    public class TaskMenagmentService
    {
        TaskService taskService = new TaskService();
        ClientService clientService = new ClientService();
        ConnectionHub notificationHub = new ConnectionHub();

        public void loadAnalysis()
        {
            //todo
        }

        public void rebildArhitecture()
        {
            //todo
        }
    }
}
