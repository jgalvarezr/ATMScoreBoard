using ATMScoreBoard.Display.Services;
using ATMScoreBoard.Shared.DTOs;
using ATMScoreBoard.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ATMScoreBoard.Display.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private Timer? _rankingTimer;

        // --- ESTADO GENERAL ---
        [ObservableProperty]
        private bool _hayPartidaEnCurso;

        [ObservableProperty]
        private string? _tipoJuegoNombre;

        // --- PANELES DE EQUIPO ---
        // Ahora tenemos dos objetos complejos que contienen toda la info de cada equipo
        [ObservableProperty]
        private TeamPanelViewModel _equipoA = new();

        [ObservableProperty]
        private TeamPanelViewModel _equipoB = new();

        // --- PANEL CENTRAL DE ESTADÍSTICAS ---
        [ObservableProperty] private int _rankPosicionA;
        [ObservableProperty] private int _rankPosicionB;
        [ObservableProperty] private int _rankPuntosA;
        [ObservableProperty] private int _rankPuntosB;
        [ObservableProperty] private int _victoriasGlobalesA;
        [ObservableProperty] private int _victoriasGlobalesB;
        [ObservableProperty] private int _victoriasH2HA;
        [ObservableProperty] private int _victoriasH2HB;

        // --- COLECCIONES PARA LOS RANKINGS ---
        public ObservableCollection<EstadisticaEquipoColRanking> RankingEquipos { get; } = new();
        public ObservableCollection<EstadisticaJugadorRanking> RankingJugadores { get; } = new();

        [ObservableProperty]
        private int _partidasParaRanking = 10;

        [ObservableProperty]
        private int _diasParaRanking = 10;

        [ObservableProperty]
        private bool _mostrandoRankingEquipos = true;

        public MainWindowViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            // Inicializamos el temporizador, pero no lo iniciamos todavía
            _rankingTimer = new Timer(AlternarVistaRanking, null, Timeout.Infinite, Timeout.Infinite);

            // Cargamos los rankings al iniciar la aplicación
            CargarRankings();
        }

        private void AlternarVistaRanking(object? state)
        {
            MostrandoRankingEquipos = !MostrandoRankingEquipos;
        }

        public async void CargarRankings()
        {
            var equipos = await _apiClient.GetRankingEquiposAsync();
            var jugadores = await _apiClient.GetRankingJugadoresAsync();
            var parametros = await _apiClient.GetRankingParamsAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                RankingEquipos.Clear();
                if (equipos != null)
                {
                    equipos.ForEach(e => RankingEquipos.Add(e));
                }

                RankingJugadores.Clear();
                if (jugadores != null)
                {
                    jugadores.ForEach(j => RankingJugadores.Add(j));
                }

                DiasParaRanking = parametros?.DiasParaRanking ?? 0;
                PartidasParaRanking = parametros?.PartidasParaRanking ?? 0;
            });

            // Inicia el temporizador para que alterne cada 10 segundos
            _rankingTimer?.Change(10000, 10000);
        }

        public void IniciarPartida(EstadoPartidaDto estadoPartida)
        {
            // Detenemos el temporizador del ranking cuando empieza una partida
            _rankingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            ActualizarEstadoPartida(estadoPartida);
        }

        public void FinalizarPartida()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                HayPartidaEnCurso = false;
                // Al finalizar, volvemos a cargar los rankings (con los nuevos datos)
                // y reiniciamos el temporizador.
                CargarRankings();
            });
        }


        // --- MÉTODO DE ACTUALIZACIÓN ---
        public void ActualizarEstadoPartida(EstadoPartidaDto estadoPartida)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                HayPartidaEnCurso = true;
                TipoJuegoNombre = ((TipoJuego)estadoPartida.TipoJuegoId).ToString();

                // --- Actualizar Panel Equipo A ---
                ActualizarTeamPanel(EquipoA, estadoPartida.EquipoA, estadoPartida.Puntuaciones["a"], estadoPartida.BolasEntroneradas["a"], estadoPartida);

                // --- Actualizar Panel Equipo B ---
                ActualizarTeamPanel(EquipoB, estadoPartida.EquipoB, estadoPartida.Puntuaciones["b"], estadoPartida.BolasEntroneradas["b"], estadoPartida);

                // --- Actualizar Panel Central ---
                RankPosicionA = estadoPartida.EquipoA.PosicionRanking;
                RankPosicionB = estadoPartida.EquipoB.PosicionRanking;
                RankPuntosA = estadoPartida.EquipoA.PuntosRanking;
                RankPuntosB = estadoPartida.EquipoB.PuntosRanking;
                VictoriasGlobalesA = estadoPartida.EquipoA.VictoriasGlobales;
                VictoriasGlobalesB = estadoPartida.EquipoB.VictoriasGlobales;
                VictoriasH2HA = estadoPartida.EquipoA.VictoriasH2H;
                VictoriasH2HB = estadoPartida.EquipoB.VictoriasH2H;



                if (estadoPartida.Ganador != Shared.DTOs.EquipoIdentifier.Ninguno)
                {
                    EquipoA.IsWinner = (estadoPartida.Ganador == Shared.DTOs.EquipoIdentifier.EquipoA);
                    EquipoB.IsWinner = (estadoPartida.Ganador == Shared.DTOs.EquipoIdentifier.EquipoB);
                }
                else
                {
                //    // Reseteamos el estado si la partida se reinicia
                    EquipoA.IsWinner = false;
                    EquipoB.IsWinner = false;
                }
            });
        }

        // --- MÉTODO DE AYUDA PARA ACTUALIZAR UN PANEL DE EQUIPO ---
        private void ActualizarTeamPanel(TeamPanelViewModel teamVM, EquipoEstadisticasDto equipoDto, int puntuacion, List<int> bolas, EstadoPartidaDto estadoPartida)
        {
            teamVM.NombreEquipo = equipoDto.Nombre;
            teamVM.Puntuacion = puntuacion;

            // Actualizar lista de jugadores
            teamVM.Jugadores.Clear();
            foreach (var jugadorDto in equipoDto.Jugadores)
            {
                teamVM.Jugadores.Add(new PlayerStatViewModel
                {
                    Nombre = jugadorDto.Nombre,
                    PosicionRanking = jugadorDto.PosicionRanking,
                    PuntosRanking = jugadorDto.PuntosRanking
                });
            }

            // Actualizar Bolas Embolsadas (Chapolín)
            teamVM.BolasEmbolsadas.Clear();
            bolas.OrderBy(b => b).ToList().ForEach(b => teamVM.BolasEmbolsadas.Add(b));

            // (La lógica para 'BolasRestantes' de Bola 8 la añadiremos después)
        }
    }
}