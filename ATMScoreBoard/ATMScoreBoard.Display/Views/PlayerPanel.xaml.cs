using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ATMScoreBoard.Display.Views
{
    /// <summary>
    /// Lógica de interacción para PlayerPanel.xaml
    /// </summary>
    public partial class PlayerPanel : UserControl
    {
        public PlayerPanel()
        {
            InitializeComponent();
        }

        public string Nombre
        {
            get { return (string)GetValue(NombreProperty); }
            set { SetValue(NombreProperty, value); }
        }
        public static readonly DependencyProperty NombreProperty =
            DependencyProperty.Register("Nombre", typeof(string), typeof(PlayerPanel), new PropertyMetadata(""));

        public int PosicionRanking
        {
            get { return (int)GetValue(PosicionRankingProperty); }
            set { SetValue(PosicionRankingProperty, value); }
        }
        public static readonly DependencyProperty PosicionRankingProperty =
            DependencyProperty.Register("PosicionRanking", typeof(int), typeof(PlayerPanel), new PropertyMetadata(0));

        public int PuntosRanking
        {
            get { return (int)GetValue(PuntosRankingProperty); }
            set { SetValue(PuntosRankingProperty, value); }
        }
        public static readonly DependencyProperty PuntosRankingProperty =
            DependencyProperty.Register("PuntosRanking", typeof(int), typeof(PlayerPanel), new PropertyMetadata(0));
    }
}
