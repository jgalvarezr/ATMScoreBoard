using ATMScoreBoard.Display.ViewModels;
using ATMScoreBoard.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace ATMScoreBoard.Display
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Vinculamos la configuración a nuestra clase
                    services.Configure<StationSettings>(hostContext.Configuration.GetSection(StationSettings.SectionName));

                    // Registramos nuestras ventanas, ViewModels y Servicios
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<TeamPanelViewModel>();
                    services.AddSingleton<PlayerStatViewModel>();
                    services.AddSingleton<SignalRService>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm.Show();

            // Iniciamos la conexión de SignalR en segundo plano
            var signalRService = AppHost.Services.GetRequiredService<SignalRService>();
            await signalRService.StartAsync();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}