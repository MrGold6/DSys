using client_wpf_app.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MyHybridApp.Helper;
using MyHybridApp.Models;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Services.StatusService;
using MyHybridApp.Services.TaskServices;
using MyHybridApp.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyHybridApp
{
    public partial class MainWindow : Window
    {
        private Timer? _resultTimer;
        private Timer _statusViewTimer;
        private Timer? logTimer;

        private Timer? _clusterBroadcastTimer;
        private static Timer? _periodicPeerSync;
        private static Timer? _peerBroadcastTimer;
        private static Timer? _taskTimer;
        private static Timer? _statisticTimer;

        public static Timer _taskCheckTimer { get; set; }
        public static Timer? _centerCheckTimer { get; set; }
        public static DateTime _becameCenterAt = DateTime.MinValue;

        private IHost? _host;

        private static readonly string _clientId = Environment.MachineName;
        private bool _isCenterStartup = true;
        static string localIp = NetworkService.GetLocalIPAddress();
        //todo можливо отримати локальний порт?
        static string localPort = "7100"; // ← змінюється для кожного вузла
        static string url = $"http://{localIp}:{localPort}";


        public MainWindow()
        {
            InitializeComponent();

            TaskRepository.CreateTaskTable();
            ResultRepository.ClearDatabaseTables();

            InitializeSignalR();
            StartStatusViewer();
            StartResultViewer();
            StartBroadcastStatusToPeers();
            StartClusterBroadcast();
            StartCountOfTasksViewer();
            StartTaskViewer();
            LoadLogFromFile();

        }

        private async void InitializeSignalR()
        {
            _host = CreateLocalHost();
            await _host.StartAsync();

            PeerStore.InitWithInitialPeer("http://172.20.10.2:7100");
            PeerStore.OwnAddress = url;
            PeerStore.ClientId = _clientId;

            if (_isCenterStartup)
            {
                IsCenterText.Text = $"Центральний вузол: {_clientId}";
                PeerStore.SetAsCenter(url);
                PeerStore.Save();
                CenterManager.StartCenterCheck();
                TaskMonitorService.StartTaskMonitorCheck();
            }
            else
            {
                IsCenterText.Text = $"Вузол: {_clientId}";
            }

            await PeerConnector.ConnectToPeersAsync(url);
            PeerStore.Announce(url);

            if (!_isCenterStartup)
            {
                await PeerConnector.RequestCenterFromPeer();
            }

            await PeerConnector.RequestTasksFromPeer();
            await PeerConnector.RequestResultsFromPeer();
            StartPeriodicPeerSync();

            Logger.Log("Component started");
        }

        private IHost CreateLocalHost()
        {
            Logger.Log($"URL: {url}");
            return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel();
                webBuilder.UseUrls(url);
                webBuilder.UseStartup<Startup>();
            })
            .Build();
        }

        //private void Log(string msg)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}  \n");
        //        LogBox.ScrollToEnd();
        //    });
        //}


        private void LoadLogFromFile()
        {
            logTimer = new Timer(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        string logContent = File.ReadAllText("app.log");
                        LogBox.Text = logContent;
                        LogBox.ScrollToEnd();
                    }
                    catch (Exception ex)
                    {
                        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Failed to read log: {ex.Message}\n");
                        LogBox.ScrollToEnd();
                    }
                });
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }


        public void StartPeriodicPeerSync()
        {
            _periodicPeerSync = new Timer(async _ =>
            {
                await PeerConnector.RequestPeersAndMergeAsync();

                PeerStore.Load();
                await PeerConnector.ConnectToPeersAsync(url);
                PeerConnector.CleanupRemovedPeers(url);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public static void StopPeriodicPeerSync()
        {
            _periodicPeerSync?.Dispose();
            _periodicPeerSync = null;
        }

        private void StartBroadcastStatusToPeers()
        {
            _peerBroadcastTimer = new Timer(async _ =>
            {
                var status = StatusMonitor.GetStatus(_clientId, url);
                ClientStatusStore.Statuses[status.ClientId] = status;

                PeerConnector.CleanupRemovedPeers(url);
                await PeerConnector.SendStatusToAllPeers(url, status);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public static void StopBroadcastStatusToPeers()
        {
            _peerBroadcastTimer?.Dispose();
            _peerBroadcastTimer = null;
        }

        private void StartResultViewer()
        {
            _resultTimer = new Timer(_ =>
            {
                List<TaskExpression> results;
                if (PeerStore.IsCenter())
                {
                    results = ResultRepository.GetAllResults();
                }
                else
                {
                    results = ResultRepository.GetResultsByClientId(_clientId);
                }


                Dispatcher.Invoke(() =>
                {
                    ResultGrid.ItemsSource = null;
                    Dispatcher.Invoke(() =>
                    {
                        ResultGrid.ItemsSource = null;

                        ResultGrid.ItemsSource = results
                            .OrderByDescending(r => r.Timestamp)
                            .Select(r => new
                            {
                                r.TargetClientId,
                                r.Result,
                                r.Expression,
                                r.Timestamp
                            })
                            .ToList();
                    });
                });

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        private void StartStatusViewer()
        {

            _statusViewTimer = new Timer(_ =>
            {
                var data = ClientStatusStore.GetAll();
                string isCenterView = "";
                Dispatcher.Invoke(() =>
                {
                    StatusGrid.ItemsSource = null;
                    StatusGrid.ItemsSource = data.Select(n => new
                    {
                        n.ClientId,
                        n.CpuUsage,
                        n.MemoryUsage,
                        isCenterView = (PeerStore.IsCenter(n.ClientUrl) ? "Center" : "Client")
                    }).ToList();
                });

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void StartTaskViewer()
        {

            _taskTimer = new Timer(_ =>
           {
               var data = TaskRepository.GetAllTasks();
               Dispatcher.Invoke(() =>
               {
                   TasksGrid.ItemsSource = null;
                   TasksGrid.ItemsSource = data.Select(n => new
                   {
                       n.TaskId,
                       n.Status,
                       n.Type,
                       n.Result,
                       n.CountOfSubTasks,
                       n.Expression,
                       n.Timestamp,
                   }).ToList();
               });

           }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void StartClusterBroadcast()
        {
            _clusterBroadcastTimer = new Timer(async _ =>
            {
                await BroadcastClusterStatus();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private async Task BroadcastClusterStatus()
        {
            try
            {
                var snapshot = new ClusterStatus
                {
                    CenterId = ClientStatusStore.CurrentCenter,
                    Nodes = ClientStatusStore.GetAll()
                };

                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText("connection_status.json", json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error with sending status: {ex.Message}");
            }
        }

        private void StartCountOfTasksViewer()
        {
            _statisticTimer = new Timer(_ =>
            {
               

                int taskCount = TaskRepository.GetAllTaskCount();
                int resultCount = ResultRepository.GetAllResultCount();
                int peerCount = PeerStore.GetPeers().Count;
                string isCenterNow = PeerStore.IsCenter() ? $"Центральний вузол: {_clientId}" : $"Вузол: {_clientId}";


                Application.Current.Dispatcher.Invoke(() =>
                {
                    PeerCountText.Text = $"Кількість вузлів: {peerCount}";
                    TaskCountText.Text = $"Кількість задач: {taskCount}";
                    TaskResultText.Text = $"Кількість проміжних результатів: {resultCount}";
                    IsCenterText.Text = isCenterNow;
                });

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        }

        public static async void DisconectPeerFromSystem(string url, string clientId)
        {
            await PeerConnector.BroadcastGoodbyeAsync(url, clientId);

            StopBroadcastStatusToPeers();
            StopPeriodicPeerSync();
            PeerStore.InitWithInitialPeer("http://172.20.10.2:7100");

            ResultRepository.ClearDatabaseTables();


            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });

        }



        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DisconectPeerFromSystem(url, _clientId);

        }

    }
}