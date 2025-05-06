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
using System.Windows.Media.Animation;


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

            connection.On("ForceDisconnect", async () =>
            {
                await connection.StopAsync();
                Dispatcher.Invoke(() =>
                {
                    MessagesList.Items.Add("Відключено сервером");
                });
                SetConnectionState(HubConnectionState.Disconnected);
            });

            //todo check if it is needed
            /*
            connection.Reconnecting += error =>
            {
                SetConnectionState(HubConnectionState.Reconnecting);
                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                SetConnectionState(HubConnectionState.Connected);
                StartStatusMonitoring();
                return Task.CompletedTask;
            };

            connection.Closed += error =>
            {
                SetConnectionState(HubConnectionState.Disconnected);
                return Task.CompletedTask;
            };
            */


            try
            {
				await connection.StartAsync();
                SetConnectionState(connection.State);
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
            if (_statusTimer != null) return; // avoid double-start

            _statusTimer = new Timer(async _ =>
            {
                try
                {
                    if (connection.State == HubConnectionState.Connected)
                    {
                        var status = GetSystemStatus();
                        await connection.InvokeAsync("ReportStatus", status);
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessagesList.Items.Add($"Report error: {ex.Message}"));
                }

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            Dispatcher.Invoke(() => MessagesList.Items.Add("Status monitoring started"));
        }

        private void StopStatusMonitoring()
        {
            _statusTimer?.Dispose();
            _statusTimer = null;
            Dispatcher.Invoke(() => MessagesList.Items.Add("Status monitoring paused"));
        }


        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                SetConnectionState(HubConnectionState.Connected);
                MessagesList.Items.Add("Already connected");
                return;
            }

            try
            {
                await connection.StartAsync();
                SetConnectionState(HubConnectionState.Connected);
                MessagesList.Items.Add(" Reconnection successful");
                StartStatusMonitoring(); // restart if needed
            }
            catch (Exception ex)
            {
                SetConnectionState(HubConnectionState.Disconnected);
                MessagesList.Items.Add($"Reconnect failed: {ex.Message}");
            }
        }

        private void UpdateConnectionStatus(string status, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                StatusIndicator.Fill = color;
                StatusLabel.Text = status;
            });
        }

        private Storyboard _blinkStoryboard;

        private void SetConnectionState(HubConnectionState state)
        {
            _blinkStoryboard = (Storyboard)FindResource("BlinkAnimation");

            switch (state)
            {
                case HubConnectionState.Connected:
                    StopBlink();
                    UpdateConnectionStatus("Connected", Brushes.Green);
                    break;
                case HubConnectionState.Reconnecting:
                case HubConnectionState.Connecting:
                    StartBlink();
                    UpdateConnectionStatus("Reconnecting...", new SolidColorBrush(Colors.Goldenrod));
                    break;
                case HubConnectionState.Disconnected:
                default:
                    StopBlink();
                    UpdateConnectionStatus("Disconnected", Brushes.Red);
                    break;
            }
        }

        private void StartBlink()
        {
            Dispatcher.Invoke(() =>
            {
                _blinkStoryboard?.Begin(StatusIndicator, true); // apply to StatusIndicator only
            });
        }

        private void StopBlink()
        {
            Dispatcher.Invoke(() =>
            {
                _blinkStoryboard?.Stop(StatusIndicator);
  
            });
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
