using client_wpf_app.Storage;
using MyHybridApp.Helper;
using MyHybridApp.Services.PeerServices;
using MyHybridApp.Storage;
using System;
using System.Threading;

namespace MyHybridApp.Services.StatusService
{
    public class CenterManager
    {
        public async static void StartCenterCheck()
        {
            MainWindow._centerCheckTimer = new Timer(_ =>
            {
                StartMonitoring();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        }


        public static void StartMonitoring()
        {

            if (PeerStore.IsManualOverride())
            {
                PeerStore.ResetManualOverride(); // перевірка таймера
                Logger.Log("Center change missed - manual mode active");
                return;
            }

            if (DateTime.UtcNow - MainWindow._becameCenterAt < TimeSpan.FromSeconds(10))
            {
                Logger.Log("Center was just assignet, wait...");
                return;
            }

            if (PeerStore.IsDistributingTasks)
            {
                Logger.Log("Center Is Distributing Tasks, wait...");
                return;
            }

            ClientStatusStore.CurrentCenter = PeerStore.OwnAddress;

            Logger.Log($"Checking the workload of the center");

            var myStatus = StatusMonitor.GetStatus(PeerStore.ClientId, PeerStore.OwnAddress);
            if (myStatus.CpuUsage > 10)
            {
                Logger.Log($"The center is overloaded " + PeerStore.OwnAddress);

                var leastLoaded = ClientStatusStore.GetLeastLoaded();
                if (leastLoaded != null)
                {
                    PeerStore.SetAsCenter(leastLoaded.ClientUrl);
                    PeerStore.Save();

                    Logger.Log($"Change of center → {leastLoaded.ClientUrl}");

                    StopCenterCheck();

                    // 🔁 повідомити інші вузли про нового центру
                    _ = PeerConnector.BroadcastNewCenterAsync(leastLoaded.ClientUrl, false);
                }
            }
        }

        public static void StopCenterCheck()
        {
            MainWindow._centerCheckTimer?.Dispose();
            MainWindow._centerCheckTimer = null;
        }

    }
}
