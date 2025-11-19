using ATMScoreBoard.Shared.DTOs;
using ATMScoreBoard.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ATMScoreBoard.Display.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
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

        // (Lógica de Rankings para el modo inactivo irá aquí después)

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