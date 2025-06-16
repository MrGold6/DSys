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
using System.Windows.Input;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.ScalarProduct
{
    public static class ScalarProductDispatcher
    {
        private static List<double> randomRange()
        {

            var rnd = new Random();
            var numbers = Enumerable.Range(1, 10).Select(i => rnd.NextDouble() * 1000).ToList();
            return numbers;
        }

        private static string randomScalar()
        {
            ScalarInputModel inputModel = new ScalarInputModel();
            inputModel.VectorA = randomRange();
            inputModel.VectorB = randomRange();

            return JsonSerializer.Serialize(inputModel);
        }

        public static async Task Start(MyTask task)
        {
            var expr = randomScalar();

            var model = JsonSerializer.Deserialize<ScalarInputModel>(expr);

            if (!PeerStore.IsCenter()) return;
            var clients = ClientStatusStore.GetOptimalClients();
            PeerStore.IsDistributingTasks = true;

            if (!clients.Any())
            {
                PeerStore.IsDistributingTasks = false;
                Logger.Log("No free clients for calculations");
                return;
            }

            int count_peers = clients.Count;
            task.CountOfSubTasks = count_peers;
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            var count = model.VectorA.Count;
            int chunkSize = (int)Math.Ceiling(count / (double)count_peers);

            int current = 0;

            foreach (var client in clients)
            {
                var subA = model.VectorA.Skip(current * chunkSize).Take(chunkSize).ToList();
                var subB = model.VectorB.Skip(current * chunkSize).Take(chunkSize).ToList();

                var chunk = new ScalarInputModel { VectorA = subA, VectorB = subB };

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


            Logger.Log("The scalar product is distributed among the nodes");
        }
    }

}
