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

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.Factorial
{
    internal class FactorialDispatcher
    {
        public static async Task Start(MyTask task)
        {
            int number = 100; // наприклад: обробка "100!"
                              // if (!task.Expression.EndsWith("!")) return;

            // if (!int.TryParse(task.Expression.TrimEnd('!'), out int n)) return;

            task.Expression = number + "!";
            TaskRepository.UpdateTask(task);
            await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            var clients = ClientStatusStore.GetOptimalClients();
            var expressions = ExpressionSplitter.SplitFactorialByNodes(number, clients.Count);

            if (!PeerStore.IsCenter()) return;

            PeerStore.IsDistributingTasks = true;


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



    }
}
