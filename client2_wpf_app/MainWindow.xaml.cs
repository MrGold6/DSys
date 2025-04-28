using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using static System.Diagnostics.Process;
using System.Threading;
using client_wpf_app.Models;


namespace client_wpf_app
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		private HubConnection connection;
		private string _username = "User2"; // Змініть на унікальне ім'я для кожного клієнта
        private Timer _statusTimer;

        public MainWindow()
		{
	
            InitializeComponent();
			InitializeSignalR();
			StartStatusMonitoring();

        }


		private async void InitializeSignalR()
		{
			connection = new HubConnectionBuilder()
				.WithUrl($"http://localhost:5282/notificationHub?username={_username}")
				.WithAutomaticReconnect()
			.Build();

			connection.On<MessageWrapper<String>>("ReceiveMessage", msg =>
			{
				Dispatcher.Invoke(() =>
				{
					MessagesList.Items.Add($"[{msg.Time:HH:mm}] {msg.Sender}: {msg.Response}");
				});
			});


			try
			{
				await connection.StartAsync();
				MessagesList.Items.Add("Підключено до сервера");
			}
			catch (Exception ex)
			{
				MessagesList.Items.Add($"Помилка підключення: {ex.Message}");
			}
		}
		private  void Button_Click(object sender, RoutedEventArgs e)
		{ }

		private async void SendButton_Click(object sender, RoutedEventArgs e)
		{
			TaskDTO taskDTO = new TaskDTO();
			var msg = new MessageWrapper<String>(MessageTextBox.Text)
            {
				Sender = _username,
				Time = DateTime.Now
			};

			try
			{
				await connection.InvokeAsync("SendToServer", msg);
			}
			catch (Exception ex)
			{
				MessagesList.Items.Add($"Помилка: {ex.Message}");
			}
		}


        private MessageWrapper<ClientStatus> GetSystemStatus()
        {
            var currentProcess = Process.GetCurrentProcess();

            var memoryMb = currentProcess.WorkingSet64 / (1024.0 * 1024.0);

            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // перше значення завжди 0
            Thread.Sleep(100); // зачекай трохи
            var cpuPercent = cpuCounter.NextValue();

            ClientStatus clientSystemStatus = new ClientStatus
            {
                CpuUsage = Math.Round(cpuPercent, 2),
                MemoryUsage = Math.Round(memoryMb, 2),
            };

            return new MessageWrapper<ClientStatus>(clientSystemStatus)
            {
                Sender = _username,
                Time = DateTime.Now
            };
        
        }


        private void StartStatusMonitoring()
        {
            _statusTimer = new Timer(async _ =>
            {
                var status = GetSystemStatus();
                //todo here will be an error when server is down
                await connection.InvokeAsync("ReportStatus", status); 

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }




        /*
		 * private async void Button_Click_1(object sender, RoutedEventArgs e)
		{
			try
			{
				await connection.InvokeAsync("SendMessage", UserTextBox.Text, MessageTextBox.Text);
			}
			catch (Exception ex)
			{
				MessagesList.Items.Add($"Помилка надсилання: {ex.Message}");
			}
		}
		*/
    }
}
