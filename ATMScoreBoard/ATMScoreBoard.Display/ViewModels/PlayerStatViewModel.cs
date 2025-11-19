using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Display.ViewModels
{
    public partial class PlayerStatViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _nombre;

        [ObservableProperty]
        private int _posicionRanking;

        [ObservableProperty]
        private int _puntosRanking;
    }
}
