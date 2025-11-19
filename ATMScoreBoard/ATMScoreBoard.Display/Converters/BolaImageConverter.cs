using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging; // Necesario para BitmapImage

namespace ATMScoreBoard.Display.Converters
{
    public class BolaImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int numeroBola)
            {
                // Construye la ruta al recurso dentro del ensamblado
                string imageName = $"bola-{numeroBola:D2}.png";
                string uriPath = $"pack://application:,,,/Images/{imageName}";

                try
                {
                    // Crea y devuelve un objeto de imagen que WPF puede renderizar
                    return new BitmapImage(new Uri(uriPath));
                }
                catch (Exception)
                {
                    // Si la imagen no se encuentra, devuelve null para no crashear
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}