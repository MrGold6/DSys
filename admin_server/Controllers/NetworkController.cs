using cental_server.Models;
using cental_server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cental_server.Controllers
{
    [Authorize]
    public class NetworkController : Controller
    {
        private readonly AdminHub _client;

        public NetworkController(AdminHub client)
        {
            _client = client;

            if (!_client.IsStarted)
                _ = _client.StartAsync();
        }

        public async Task<IActionResult> IndexAsync()
        {
            var model = new NetworkViewModel
            {
                Statuses = await _client.FetchStatusesAsync(),
                Peers = await _client.FetchPeersAsync(),
                CurrentCenter = await _client.FetchCenterAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> StatusPartialAsync()
        {
            var model = new NetworkViewModel
            {
                Statuses = await _client.FetchStatusesAsync(),
                Peers = await _client.FetchPeersAsync(),
                CurrentCenter = await _client.FetchCenterAsync()
            };

            return PartialView("StatusPartial", model);
        }


        [HttpPost]
        public async Task<IActionResult> SetCenter(string newCenter)
        {
            Console.WriteLine("nc" + newCenter);
            if (!string.IsNullOrWhiteSpace(newCenter))
            {
                await _client.SetCenter(newCenter);
            }

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> DisconnectNode(string url, string clientId)
        {
            await _client.DisconnectPeer(url, clientId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendTask(MyTaskType type)
        {
            Console.WriteLine("SendTask");
            //expression string???
            var task = new MyTask
            {
                TaskId = Guid.NewGuid().ToString(),
                //Expression = expression,
                Status= MyTaskStatus.ToDo,
                Type= type,
                Timestamp = DateTime.UtcNow
            };

            await _client.SendTask(task);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> TaskAsync()
        {
            var model = new NetworkViewModel
            {
                Tasks = await _client.FetchTasksAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> TasksPartialAsync()
        {
            var model = new NetworkViewModel
            {
                Tasks = await _client.FetchTasksAsync()
            };

            return PartialView("TasksPartial", model);
        }

        public async Task<IActionResult> TaskExpression(string taskId)
        {
            MyTask task = await _client.FetchTaskByIdAsync(taskId);
            List<TaskExpression> taskExpressions = await _client.FetchTaskExpressionAsync(taskId);

            var model = new TaskViewModel
            {
                Task = task,
                SubTask = taskExpressions,
            };

            return View(model);
        }

        public async Task<IActionResult> TasksExpressionPartial(string taskId)
        {
            MyTask task = await _client.FetchTaskByIdAsync(taskId);
            List<TaskExpression> taskExpressions = await _client.FetchTaskExpressionAsync(taskId);

            var model = new TaskViewModel
            {
                Task = task,
                SubTask = taskExpressions,
            };

            return PartialView("TasksExpressionPartial", model);
        }



    }

}
