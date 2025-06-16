using Microsoft.Extensions.Hosting;
using System.Windows;

namespace MyHybridApp
{
    public partial class App : Application
    {
        private IHost _webHost;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_webHost != null)
                await _webHost.StopAsync();

            base.OnExit(e);
        }
    }
}