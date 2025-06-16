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
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.MultiplicationOfBigNumbers
{
    public class MultiplicationOfBigNumbersDispatcher
    {
        public static async Task Start(MyTask task)
        {
            var expressions = GenerateExpressions(30);
            var fullExpression = "";
            foreach (var expression in expressions)
            {
                fullExpression += expression + " * ";
            }

            task.Expression = fullExpression;
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            if (!PeerStore.IsCenter()) return;

            PeerStore.IsDistributingTasks = true;

            var clients = ClientStatusStore.GetOptimalClients();

            if (!clients.Any())
            {
                Logger.Log("No free clients for calculations");
                return;
            }

            task.CountOfSubTasks = expressions.Count;
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            int current = 0;

            foreach (var client in clients)
            {
                int tasksPerClient = expressions.Count / clients.Count;

                for (int i = 0; i < tasksPerClient && current < expressions.Count; i++)
                {
                    var expr = expressions[current++];
                    var taskDestr = new TaskExpression
                    {
                        TaskId = task.TaskId,
                        Expression = expr,
                        Type = task.Type,
                        TargetClientId = client.ClientId
                    };

                    await PeerConnector.SendTaskTo(client.ClientUrl, taskDestr);


                    Logger.Log($"Send [{task.Expression}] to component {client.ClientId}");
                }
            }
            PeerStore.IsDistributingTasks = false;
        }

        private static List<string> GenerateExpressions(int count)
        {
            var list = new List<string>();
            var rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                long a = rnd.NextInt64(1_000_000_000, 9_999_999_999);
                long b = rnd.NextInt64(1_000_000_000, 9_999_999_999);
                long c = rnd.NextInt64(1_000_000_000, 9_999_999_999);
                list.Add($"{a} + {b} - {c}");
            }

            return list;
        }
    }
}
