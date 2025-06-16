using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.EvenNumber
{
    public static class EvenNumberDispatcher
    {
        private static string randomRange()
        {
            var rnd = new Random();
            var numbers = Enumerable.Range(0, 1000).Select(_ => rnd.Next(0, 1000)).ToList();
            string json = JsonSerializer.Serialize(numbers);
            return json;
        }

        public static async Task Start(MyTask task)
        {
            var expr = randomRange();
            var numbers = JsonSerializer.Deserialize<List<double>>(expr);
            if (numbers == null || numbers.Count == 0) return;

            if (!PeerStore.IsCenter()) return;
            var clients = ClientStatusStore.GetOptimalClients();
            PeerStore.IsDistributingTasks = true;

            if (!clients.Any())
            {
                PeerStore.IsDistributingTasks = false;
                Logger.Log("No free clients for calculations");
                return;
            }

            int count = clients.Count;
            task.CountOfSubTasks = count;
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            int chunkSize = (int)Math.Ceiling(numbers.Count / (double)count);

            int current = 0;

            foreach (var client in clients)
            {
                var chunk = numbers.Skip(current * chunkSize).Take(chunkSize).ToList();

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


            Logger.Log($"The EvenFilter task is distributed among {count} nodes");
        }
    }

}
