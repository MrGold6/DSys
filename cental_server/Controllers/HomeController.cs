using cental_server.Models;
using cental_server.Services;
using cental_server.Storages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace cental_server.Controllers
{
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHubContext<ConnectionHub> _hubContext;


		public HomeController(ILogger<HomeController> logger, IHubContext<ConnectionHub> hubContext)
		{
			_logger = logger;
			_hubContext = hubContext;
		}

		public IActionResult Index()
		{
			return View(ClientService.GetAllStatuses().Values);
		}

        public IActionResult Privacy()
        {
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpPost("send")]
		public async Task<IActionResult> SendToUser([FromQuery] string username, [FromQuery] string message)
		{
			// Отримуємо connectionId з NotificationHub (наприклад, через статичний словник або окрему службу)
			var connectionId = ConnectionHub.GetConnectionIdByUsername(username);
			if (connectionId == null)
				return NotFound("Користувача не знайдено");

            MessageWrapper<string> chatMessage = new MessageWrapper<string>(message)
			{
				Sender = "server",
				Time = DateTime.Now
			};
			await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", chatMessage);
			return Ok("Повідомлення надіслано");
		}

		[HttpGet("messages")]
		public IActionResult GetMessages()
		{
			return Ok(MessageStore.GetAll());
		}

        [HttpGet("clients")]
        public IActionResult GetStatuses()
        {
            return Ok(ClientService.GetAllStatuses().Values);
        }

        [HttpGet("clientStatus")]
        public IActionResult GetStatuses([FromQuery] string username)
        {
            return Ok(ClientService.GetStatusByUser(username));
        }

        [HttpGet("count_of_clients")]
        public IActionResult GetCountOfClients()
        {
            return Ok(ClientService.GetCountOfClients());
        }

        [HttpGet("Home/DisconnectClient/{username}")]
        public async Task<IActionResult> DisconnectClient(string username)
        {
            var connectionId = ConnectionHub.GetConnectionIdByUsername(username);
            if (connectionId == null)
                return NotFound("Користувач не знайдений або не підключений");

            await _hubContext.Clients.Client(connectionId).SendAsync("ForceDisconnect");

            ClientStorage._userConnections.TryRemove(username, out _);
            ClientStorage._clientStatus.TryRemove(username, out _);

			Console.WriteLine($"Клієнт '{username}' відключений");
            return RedirectToAction(nameof(Index));
        }
    }


}
