using client_wpf_app.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Management;
using MyHybridApp.Helper;
using MyHybridApp.Storage;

namespace MyHybridApp.Services.StatusService
{
    public static class StatusMonitor
    {
        public static ulong GetTotalMemoryInMb()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                return (ulong)obj["TotalVisibleMemorySize"] / 1024; // в МБ
            }
            return 0;
        }


        public static ClientStatus GetStatus(string id, string url)
        {
            // CPU
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            Thread.Sleep(100);
            float cpuUsage = cpuCounter.NextValue();

            // RAM
            var process = Process.GetCurrentProcess();
            double memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);

            var cpu = Math.Round(cpuUsage, 2);
            var memory = Math.Round(memoryMB, 2);
            var totalMemory = GetTotalMemoryInMb();

            Logger.Log($"Status: CPU {cpu}% | RAM {memory}MB");

            return new ClientStatus
            {
                ClientId = id,
                ClientUrl = url,
                CpuUsage = cpu,
                MemoryUsage = memory,
                Timestamp = DateTime.Now,
                TotalMemoryMB = totalMemory,
                IsCenter = PeerStore.IsCenter(),
            };
        }
    }

}
