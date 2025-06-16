using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Services.TaskServices.TaskDispatcher.ScalarProduct;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.DenseMatrix
{
    public static class DenseMatrixDispatcher
    {
        public static string randomMatrix() {

            int N = 10; 
            var rand = new Random();

            var A = Enumerable.Range(0, N)
                .Select(_ => Enumerable.Range(0, N)
                .Select(_ => rand.NextDouble() * 100).ToList())
                .ToList();

            var B = Enumerable.Range(0, N)
                .Select(_ => Enumerable.Range(0, N)
                .Select(_ => rand.NextDouble() * 100).ToList())
                .ToList();

            var options = new JsonSerializerOptions
            {
                MaxDepth = 0,
                WriteIndented = false
            };
            var expression = JsonSerializer.Serialize(new DenseMatrixModel
            {
                ARows = A,
                B = B,
                StartRowIndex = 0
            }, options);

            return expression;
        }
        public static async Task Start(MyTask task)
        {
            var expr = randomMatrix();

            var model = JsonSerializer.Deserialize<DenseMatrixModel>(expr);

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

            var A = model.ARows; // в цьому випадку це вся матриця A
            var B = model.B;
            int rowsPerNode = (int)Math.Ceiling(A.Count / (double)count_peers);


            int current = 0;

            var options = new JsonSerializerOptions
            {
                MaxDepth = 0,
                WriteIndented = false
            };

            foreach (var client in clients)
            {
                var startRow = current * rowsPerNode;
                var rows = A.Skip(startRow).Take(rowsPerNode).ToList();

                if (rows.Count == 0)
                    continue;

                var chunkModel = new DenseMatrixModel
                {
                    ARows = rows,
                    B = B,
                    StartRowIndex = startRow
                };

                var taskDestr = new TaskExpression
                {
                    TaskId = task.TaskId,
                    Expression = JsonSerializer.Serialize(chunkModel, options),
                    Type = task.Type,
                    TargetClientId = client.ClientId
                };


                await PeerConnector.SendTaskTo(client.ClientUrl, taskDestr);

                current++;

            }
            PeerStore.IsDistributingTasks = false;

            Logger.Log($"Dense matrix multiplication is distributed between {count_peers} nodes");
        }
    }

}
