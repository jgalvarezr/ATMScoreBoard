using ATMScoreBoard.Display.ViewModels;
using System.Windows;

namespace ATMScoreBoard.Display
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}