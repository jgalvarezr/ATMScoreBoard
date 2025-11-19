using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ATMScoreBoard.Display.ViewModels
{
    public partial class TeamPanelViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _nombreEquipo;

        [ObservableProperty]
        private int _puntuacion;

        // Lista de jugadores con sus estadísticas
        public ObservableCollection<PlayerStatViewModel> Jugadores { get; } = new();

        // Lista de bolas para Chapolín
        public ObservableCollection<int> BolasEmbolsadas { get; } = new();

        // Lista de bolas para Bola 8
        public ObservableCollection<int> BolasRestantes { get; } = new();
                
        public bool IsPareja => Jugadores.Count > 1;

    }
}
