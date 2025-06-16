using client_wpf_app.Models;
using client_wpf_app.Storage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.StatusService;
using MyHybridApp.Services.TaskServices;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MinMax;
using MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev;
using MyHybridApp.Services.TaskServices.TaskDispatcher.Sum;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;

namespace MyHybridApp.Services.PeerServices
{
    public class PeerHub : Hub
    {
        private static readonly List<TaskExpression> _results = new();

        //peer managment

        public Task AnnouncePeer(string address)
        {
            PeerStore.Announce(address);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetPeers()
        {
            return Task.FromResult(PeerStore.All.ToList());
        }

        //status section
        public Task<List<ClientStatus>> GetStatuses()
        {
            return Task.FromResult(ClientStatusStore.GetAll());
        }

        public Task ReceivePeerStatus(ClientStatus status)
        {
            ClientStatusStore.Add(status);
            return Task.CompletedTask;
        }

        //center section
        public Task<string> GetCenter()
        {
            return Task.FromResult(PeerStore.CurrentCenter);
        }

        //todo  add broadcast center
        public Task ReceiveManualyCenter(string newCenter)
        {
            Logger.Log($"A new manual center was received: {newCenter}");

            if (PeerStore.CurrentCenter != newCenter)
            {

                PeerStore.SetAsCenter(newCenter);
                PeerStore.Save();
                TaskMonitorService.StopTaskMonitorCheck();

                if (PeerStore.IsCenter())
                {
                    PeerStore.SetManualOverride(true);
                    MainWindow._becameCenterAt = DateTime.UtcNow;
                    CenterManager.StartCenterCheck();
                    TaskMonitorService.StartTaskMonitorCheck();
                }

                _ = PeerConnector.BroadcastNewCenterAsync(newCenter, true);
            }

            return Task.CompletedTask;
        }

        public Task ReceiveNewCenter(string newCenter, bool isManual)
        {
            Logger.Log($"A new center was received: {newCenter}");

            if (PeerStore.IsCenter())
            {
                CenterManager.StopCenterCheck();
                TaskMonitorService.StopTaskMonitorCheck();
            }

            PeerStore.SetAsCenter(newCenter);
            PeerStore.Save();

            if (PeerStore.IsCenter())
            {
                PeerStore.SetManualOverride(isManual);
                MainWindow._becameCenterAt = DateTime.UtcNow;
                CenterManager.StartCenterCheck();
                TaskMonitorService.StartTaskMonitorCheck();
            }
            return Task.CompletedTask;
        }

        //task section
        public async Task ReceiveAdminTask(MyTask task)
        {
            Logger.Log($"Task received from the administrator: {task.Expression}");

            await TaskRepository.AddOrUpdateTask(task);
            //mb fix: add timeout????
            await PeerConnector.SendTaskFromAdminAllPeers(task);


            //if (PeerStore.IsCenter())
            //{
            //    var peers = ClientStatusStore.GetOptimalClients().Count;
            //    //if (peers > 0)
            //    //{
            //    //    Logger.Log("✅ Я є центр, я розподіляю завданняя.");
            //    //    //todo або видалити і залишити таймер, або прибрати таймер
            //    //    task.setStatus(MyTaskStatus.InProgress);
            //    //    TaskRepository.UpdateStatus(task.TaskId, task.Status);

            //    //    await PeerConnector.SendStatusOfTaskToOtherPeers(task);
            //    //    await TaskDistributor.DistributeTaskByType(task);
            //    //}
            //}

        }

        public async Task SaveTaskFromAdmin(MyTask task)
        {
            Logger.Log($"Task received: {task.Expression}");

            TaskRepository.AddOrUpdateTask(task);

            //if (PeerStore.IsCenter())
            //{
            //    var peers = ClientStatusStore.GetOptimalClients().Count;
            //    Logger.Log("peersCount: " + peers);
            //    //if (peers > 0)
            //    //{
            //    //    Logger.Log("✅ Я є центр, я розподіляю завданняя.");

            //    //    task.setStatus(MyTaskStatus.InProgress);
            //    //    TaskRepository.UpdateStatus(task.TaskId, task.Status);

            //    //    await PeerConnector.SendStatusOfTaskToOtherPeers(task);
            //    //    await TaskDistributor.DistributeTaskByType(task);
            //    //}
            //}
        }

        public Task<List<MyTask>> GetAllTasks()
        {
            return Task.FromResult(TaskRepository.GetAllTasks());
        }

        public Task<List<TaskExpression>> GetAllResults()
        {
            return Task.FromResult(ResultRepository.GetAllResults());
        }


        public Task ReceiveTask(TaskExpression task)
        {
            Logger.Log($"Got task: {task.Expression}");

            task.Result = CalculateResult.getCalulatedPartialResultByType(task);
            Logger.Log($"Result: {task.Result}");

            ResultRepository.SaveResult(task);
            _ = PeerConnector.SendResultToAllPeers(task);

            return Task.CompletedTask;

        }

        public static List<TaskExpression> GetResults()
        {
            lock (_results)
            {
                return _results.ToList();
            }
        }

       

        public async Task ReceiveResult(TaskExpression model)
        {
            Logger.Log($"Result {model.TaskId} from {model.TargetClientId}: {model.Result}");
            model.Timestamp = DateTime.Now;
           
            lock (_results)
            {
                _results.Add(model);
            }

            ResultRepository.SaveResult(model);

            //do I need it here????
            //if (PeerStore.IsCenter())
            //{
            //    //change expected!!!!!
            //    task.CountOfSubTasks;
            //    int received = ResultRepository.GetResultCount(model.TaskId);

            //    if (received >= expected - 1)
            //    {
            //        MyTask task = TaskMonitorService.calcalateFinalResult(model.TaskId);
            //        await PeerConnector.SendStatusOfTaskToOtherPeers(task);

            //    }
            //}
        }

        public Task ReceiveTaskUpdate(MyTask task)
        {
            Logger.Log($"Update the task status {task.TaskId} → {task.Status}");
            TaskRepository.UpdateTask(task);
            return Task.CompletedTask;
        }


        public Task DeleteResultByTaskId(string taskId)
        {
            Logger.Log($"Delete results for {taskId} ");
            ResultRepository.DeleteAllResultsByTaskId(taskId);
            return Task.CompletedTask;
        }

        public Task<List<MyTask>> GetTasks()
        {
            return Task.FromResult(TaskRepository.GetAllTasks());
        }

        public Task<MyTask> GetTaskByIdAdmin(string taskId)
        {
            return Task.FromResult(TaskRepository.GetTaskById(taskId));
        }

        public Task<List<TaskExpression>> GetTaskExpressionAdmin(string taskId)
        {
            return Task.FromResult(ResultRepository.GetResultsByTaskId(taskId));
        }

        //disconect section
        public Task Goodbye(string address, string clientId)
        {
            Logger.Log($"Node {address} is offline");
            PeerStore.Remove(address);
            ClientStatusStore.Remove(clientId);
            return Task.CompletedTask;
        }


        public async Task ManualDisconnectPeer(string url, string clientId)
        {
            Logger.Log($"The administrator has disabled the node: {url}");

            if (PeerStore.OwnAddress == url)
            {
                MainWindow.DisconectPeerFromSystem(url, clientId);
            }
            else {
                await PeerConnector.SendManualDisconnectTo(url, clientId);
            }

        }

        public Task RemoteDisconnect(string url, string clientId)
        {
            Logger.Log($"The administrator has disabled the node: {url}");

            MainWindow.DisconectPeerFromSystem(url, clientId);

            return Task.CompletedTask;
        }

    }

}
