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

            // 1. Evento para INICIAR o ACTUALIZAR una partida
            _hubConnection.On<EstadoPartidaDto>("PartidaActualizada", (estadoPartida) =>
            {
                Debug.WriteLine($"[SignalR] Partida Actualizada recibida!");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _viewModel.ActualizarEstadoPartida(estadoPartida);
                });
            });

            // 2. Evento para FINALIZAR una partida
            _hubConnection.On("PartidaFinalizada", () =>
            {
                Debug.WriteLine($"[SignalR] Mensaje 'PartidaFinalizada' recibido.");

                // El ViewModel tiene un método para volver al estado 'Inactivo',
                // cargar los rankings de nuevo y activar el timer.
                _viewModel.FinalizarPartida();
            });

            _hubConnection.On("RankingsActualizados", () =>
            {
                Debug.WriteLine("[SignalR] Notificación 'RankingsActualizados' recibida.");

                // Llamamos al método del ViewModel que recarga los rankings
                viewModel.CargarRankings();
            });


            // (Opcional) Manejar la desconexión y reconexión para depuración
            _hubConnection.Closed += (error) =>
            {
                Debug.WriteLine($"[SignalR] Conexión cerrada: {error?.Message}");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) =>
            {
                Debug.WriteLine($"[SignalR] Intentando reconectar... {error?.Message}");
                return Task.CompletedTask;
            };

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