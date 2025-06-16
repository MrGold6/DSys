using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.MinMax
{
    public static class MinMaxDispatcher
    {
        private static string randomRange()
        {

            var rnd = new Random();
            var numbers = Enumerable.Range(1, 10000).Select(i => rnd.NextDouble() * 1000).ToList();
            string json = JsonSerializer.Serialize(numbers);
            return json;
        }
        public static async Task Start(MyTask task)
        {
            var expr = randomRange();

            var values = JsonSerializer.Deserialize<List<double>>(expr);

            if (!PeerStore.IsCenter()) return;
            var clients = ClientStatusStore.GetOptimalClients();

            if (!clients.Any())
            {
                Logger.Log("No free clients for calculations");
                return;
            }

            PeerStore.IsDistributingTasks = true;

            int count = clients.Count;
            task.CountOfSubTasks = count;
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            int chunkSize = (int)Math.Ceiling(values.Count / (double)count);


            task.Expression = JsonSerializer.Serialize(values);
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            int current = 0;

            foreach (var client in clients)
            {
                var chunk = values.Skip(current * chunkSize).Take(chunkSize).ToList();

                var taskDestr = new TaskExpression
                {
                    TaskId = task.TaskId,
                    Expression = JsonSerializer.Serialize(chunk),
                    Type = task.Type,
                    TargetClientId = client.ClientId
                };

                await PeerConnector.SendTaskTo(client.ClientUrl, taskDestr);

                current++;
            }
            PeerStore.IsDistributingTasks = false;



            Logger.Log($"Divided the task {task.Type} into {count} parts");
        }
    }

}
