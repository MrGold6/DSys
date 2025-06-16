using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev
{

    public static class StdDevDispatcher
    {
        private static string randomRange() {

            var rnd = new Random();
            var numbers = Enumerable.Range(0, 100).Select(_ => rnd.Next(0, 1000)).ToList();
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

            int chunkSize = (int)Math.Ceiling(numbers.Count / (double)count);

            var partialSums = new List<(double Sum, int Count)>();

            // Step 1: Calculate Mean
            for (int i = 0; i < count; i++)
            {
                var chunk = numbers.Skip(i * chunkSize).Take(chunkSize).ToList();
                double sum = chunk.Sum();
                partialSums.Add((sum, chunk.Count));
            }

            double totalSum = partialSums.Sum(p => p.Sum);
            int totalCount = partialSums.Sum(p => p.Count);
            double mean = totalSum / totalCount;

            // Step 2: Distribute variance calculations
            task.CountOfSubTasks = count;
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);


            var expressionModel = new StdDevExpressionModel
            {
                Values = numbers,
                TotalCount = totalCount,
            };

            task.Expression = JsonSerializer.Serialize(expressionModel);
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            int current = 0;

            foreach (var client in clients)
            {
                var chunk = numbers.Skip(current * chunkSize).Take(chunkSize).ToList();
                var model = new StdDevExpressionModel
                {
                    Values = chunk,
                    Mean = mean,
                    TotalCount = totalCount,
                };


                var taskDestr = new TaskExpression
                {
                    TaskId = task.TaskId,
                    Expression = JsonSerializer.Serialize(model),
                    Type = task.Type,
                    TargetClientId = client.ClientId
                };

                await PeerConnector.SendTaskTo(client.ClientUrl, taskDestr);

                current++;
                Logger.Log($"The StdDev task has started with {numbers.Count} values, Mean = {mean}");

            }
            PeerStore.IsDistributingTasks = false;

        }
    }
    

}
