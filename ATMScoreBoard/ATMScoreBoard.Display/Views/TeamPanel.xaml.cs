using CommunityToolkit.Mvvm.ComponentModel;
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
    /// Lógica de interacción para TeamPanel.xaml
    /// </summary>
    public partial class TeamPanel : UserControl
    {
        public TeamPanel()
        {
            InitializeComponent();
        }
        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }


        public static readonly System.Windows.DependencyProperty BorderColorProperty =
            System.Windows.DependencyProperty.Register("BorderColor",
                typeof(Brush),
                typeof(TeamPanel),
                new PropertyMetadata(new SolidColorBrush(Colors.Gray)));
    }
}
