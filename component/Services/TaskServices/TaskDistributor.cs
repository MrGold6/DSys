using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Services.TaskServices.TaskDispatcher.DenseMatrix;
using MyHybridApp.Services.TaskServices.TaskDispatcher.EvenNumber;
using MyHybridApp.Services.TaskServices.TaskDispatcher.Factorial;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MinMax;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MultiplicationOfBigNumbers;
using MyHybridApp.Services.TaskServices.TaskDispatcher.ScalarProduct;
using MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev;
using MyHybridApp.Services.TaskServices.TaskDispatcher.Sum;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices
{
    public class TaskDistributor
    {


        public static TimeSpan getExpirationTimeByType(MyTask task)
        {
            switch (task.Type)
            {
                case MyTaskType.Factorial:
                    {
                        return TimeSpan.FromSeconds(100);

                    }
                case MyTaskType.MultiplyBigNumbers:
                    {
                        return TimeSpan.FromSeconds(40);
                    }
                case MyTaskType.StdDev:
                    {
                        return TimeSpan.FromMinutes(3);
                    }
                case MyTaskType.Sum:
                    {
                        return TimeSpan.FromMinutes(1);
                    }
                case MyTaskType.EvenFilter:
                    {
                        return TimeSpan.FromMinutes(2);
                    }
                case MyTaskType.ScalarProduct:
                    {
                        return TimeSpan.FromMinutes(2);
                    }
                case MyTaskType.DenseMatrixMultiply:
                    {
                        return TimeSpan.FromMinutes(3);
                    }
                default: return TimeSpan.FromSeconds(40);

            }
        }
        public static async Task DistributeTaskByType(MyTask task)
        {
            var peers = ClientStatusStore.GetOptimalClients().Count;
            Logger.Log("peersCount: " + peers);
            if (peers > 0)
            {
                switch (task.Type)
                {
                    case MyTaskType.Factorial:
                        {
                            await FactorialDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.MultiplyBigNumbers:
                        {
                            await MultiplicationOfBigNumbersDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.StdDev:
                        {
                            await StdDevDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.Min:
                        {
                            await MinMaxDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.Max:
                        {
                            await MinMaxDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.Sum:
                        {
                            await SumDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.EvenFilter:
                        {
                            await EvenNumberDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.ScalarProduct:
                        {
                            await ScalarProductDispatcher.Start(task);
                            break;
                        }
                    case MyTaskType.DenseMatrixMultiply:
                        {
                            await DenseMatrixDispatcher.Start(task);
                            break;
                        }

                }
            }


        }

        //public static async Task DistributeTasksAsync(MyTask task, List<string> expressions)
        //{
        //    if (!PeerStore.IsCenter()) return;

        //    PeerStore.IsDistributingTasks = true;

        //    var clients = ClientStatusStore.GetOptimalClients();

        //    if (!clients.Any())
        //    {
        //        Logger.Log("⚠️ No free clients for calculations");
        //        return;
        //    }

        //    task.CountOfSubTasks = expressions.Count;
        //    TaskRepository.UpdateTask(task);
        //    await PeerConnector.SendStatusOfTaskToOtherPeers(task);

        //    int current = 0;

        //    foreach (var client in clients)
        //    {
        //        int tasksPerClient = expressions.Count / clients.Count;

        //        for (int i = 0; i < tasksPerClient && current < expressions.Count; i++)
        //        {
        //            var expr = expressions[current++];
        //            var taskDestr = new TaskExpression
        //            {
        //                TaskId = task.TaskId,
        //                Expression = expr,
        //                TargetClientId = client.ClientId
        //            };

        //            await PeerConnector.SendTaskTo(client.ClientUrl, taskDestr);


        //            Logger.Log($"📤 Send [{task.Expression}] to component {client.ClientId}");
        //        }
        //    }
        //    PeerStore.IsDistributingTasks = false;


        //}

       



    }

}
