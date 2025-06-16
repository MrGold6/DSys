using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Services.TaskServices.TaskDispatcher.Factorial;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MinMax;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MultiplicationOfBigNumbers;
using MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev;
using MyHybridApp.Storage;
using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices
{
    public class TaskMonitorService
    {

        public static void StartTaskMonitorCheck()
        {
            MainWindow._taskCheckTimer = new Timer(_ =>
            {
                TaskMonitoring();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public static void StopTaskMonitorCheck()
        {
            MainWindow._taskCheckTimer?.Dispose();
            MainWindow._taskCheckTimer = null;
        }

        public static async void TaskMonitoring()
        {
            if (!PeerStore.IsCenter())
                return;


            var tasks = TaskRepository.GetAllNotReadyTasks();

            // 1. Перевірити чи є InProgress задачі певного типу
            var inProgressTypes = tasks
                .Where(t => t.Status == MyTaskStatus.InProgress)
                .Select(t => t.Type)
                .Distinct()
                .ToList();

            // 2. Видалити ToDo задачі, якщо вже є InProgress такого типу
            tasks = tasks
                .Where(t =>
                    t.Status != MyTaskStatus.ToDo ||
                    !inProgressTypes.Contains(t.Type))
                .ToList();

            // 3. Якщо кілька ToDo одного типу — залишити одну найстарішу
            var todoByType = tasks
                .Where(t => t.Status == MyTaskStatus.ToDo)
                .GroupBy(t => t.Type)
                .Select(g => g.OrderBy(t => t.Timestamp).First());

            var notTodo = tasks.Where(t => t.Status != MyTaskStatus.ToDo);
            tasks = notTodo.Concat(todoByType).ToList();



            foreach (var task in tasks)
            {
                var received = ResultRepository.GetResultCount(task.TaskId);
                var expected = task.CountOfSubTasks;

                switch (task.Status)
                {
                    case (MyTaskStatus.ToDo):
                        {
                            var peers = ClientStatusStore.GetOptimalClients().Count;
                            if (peers > 0)
                            {
                                //todo make 2 timestamp for enter system and in progress
                                task.Timestamp = DateTime.UtcNow;
                                task.setStatus(MyTaskStatus.InProgress);
                                TaskRepository.UpdateTask(task);

                                await PeerConnector.SendStatusOfTaskToOtherPeers(task);

                                await TaskDistributor.DistributeTaskByType(task);
                            }

                            break;
                        }
                    case (MyTaskStatus.InProgress):
                        {
                            Logger.Log("Checking");
                            if (received >= expected)
                            {
                               
                                MyTask finalResult = await calcalateFinalResultAsync(task.TaskId);
                                
                                await PeerConnector.SendStatusOfTaskToOtherPeers(finalResult);
                                break;
                            }

                            //todo change time for more

                            var expirationTime = TaskDistributor.getExpirationTimeByType(task);
                            var timeOfTask = DateTime.UtcNow.TimeOfDay - task.Timestamp.TimeOfDay;
                            Logger.Log("Checking" + timeOfTask);
                            if (timeOfTask > expirationTime)
                            {
                                Logger.Log($"Timeout for task {task.TaskId}");
                                task.setStatus(MyTaskStatus.Failure);
                                TaskRepository.UpdateStatus(task.TaskId, task.Status);

                                await PeerConnector.SendStatusOfTaskToOtherPeers(task);
                            }


                            break;
                        }
                    case (MyTaskStatus.Failure):
                        {
                            var expirationTime = TaskDistributor.getExpirationTimeByType(task);
                            if (task.Timestamp.TimeOfDay > expirationTime.Add(TimeSpan.FromSeconds(40)))
                            {
                                Logger.Log($"Start error task again {task.TaskId}");
                                ResultRepository.DeleteAllResultsByTaskId(task.TaskId);
                                await PeerConnector.DeleteAllResultsByTaskIdToOtherPeers(task.TaskId);

                                task.setStatus(MyTaskStatus.ToDo);
                                TaskRepository.UpdateTask(task);
                                await PeerConnector.SendStatusOfTaskToOtherPeers(task);
                            }
                            break;
                        }

                }

            }

        }

        public static async Task<MyTask> calcalateFinalResultAsync(string TaskId)
        {
            Logger.Log($"Task {TaskId} is complete (all results have been received)");

            var allResults = ResultRepository.GetResultsByTaskId(TaskId);
            

            MyTask task = TaskRepository.GetTaskById(TaskId);
            Logger.Log($"Final expression for calculation");

            task.Result = CalculateResult.getCalulatedFinalResultByType(task, allResults);
            Logger.Log($"Result {task.Result}");
           
            task.Status = MyTaskStatus.Success;

            task.Timestamp = DateTime.Now;

            Logger.Log("Task result on saving final" + task.Result);

            TaskRepository.UpdateTask(task);

            return task;
        }
    }
}

