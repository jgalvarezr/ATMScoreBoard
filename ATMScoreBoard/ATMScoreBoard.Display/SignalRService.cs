using ATMScoreBoard.Display.ViewModels;
using ATMScoreBoard.Shared.Configuration;
using ATMScoreBoard.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ATMScoreBoard.Display
{
    public class SignalRService
    {
        private readonly HubConnection _hubConnection;
        private readonly StationSettings _settings;
        private readonly MainWindowViewModel _viewModel;

        public SignalRService(IOptions<StationSettings> settings, MainWindowViewModel viewModel)
        {
            _settings = settings.Value;
            _viewModel = viewModel; // Guardamos una referencia al ViewModel

            var hubUrl = $"{_settings.ApiBaseUrl.TrimEnd('/')}/marcadorhub";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();


            _hubConnection.On<EstadoPartidaDto>("PartidaActualizada", (estadoPartida) =>
            {
                Debug.WriteLine($"[SignalR] Partida Actualizada recibida!");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _viewModel.ActualizarEstadoPartida(estadoPartida);
                });
            });
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                Debug.WriteLine("[SignalR] Conexión establecida.");

                await _hubConnection.InvokeAsync("JoinMesaGroup", _settings.MesaId.ToString());
                Debug.WriteLine($"[SignalR] Unido al grupo de la Mesa N° {_settings.MesaId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error al conectar: {ex.Message}");
            }
        }
    }
}