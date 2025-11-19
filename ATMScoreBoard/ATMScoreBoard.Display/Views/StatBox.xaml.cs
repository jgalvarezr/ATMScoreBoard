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
    /// Lógica de interacción para StatBox.xaml
    /// </summary>
    public partial class StatBox : UserControl
    {
        public StatBox()
        {
            InitializeComponent();
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(StatBox), new PropertyMetadata(""));

        public int ValueA
        {
            get { return (int)GetValue(ValueAProperty); }
            set { SetValue(ValueAProperty, value); }
        }
        public static readonly DependencyProperty ValueAProperty =
            DependencyProperty.Register("ValueA", typeof(int), typeof(StatBox), new PropertyMetadata(0));

        public int ValueB
        {
            get { return (int)GetValue(ValueBProperty); }
            set { SetValue(ValueBProperty, value); }
        }
        public static readonly DependencyProperty ValueBProperty =
            DependencyProperty.Register("ValueB", typeof(int), typeof(StatBox), new PropertyMetadata(0));
    }
}
