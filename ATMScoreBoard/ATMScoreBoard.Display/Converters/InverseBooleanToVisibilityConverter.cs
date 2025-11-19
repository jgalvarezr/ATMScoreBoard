using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ATMScoreBoard.Display.Converters
{
    // Este convertidor traduce un valor booleano a su valor de Visibilidad INVERSO.
    // true -> Collapsed
    // false -> Visible
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Primero, nos aseguramos de que el valor de entrada es un booleano.
            if (value is bool boolValue)
            {
                // Si el valor es 'true', queremos que esté OCULTO (Collapsed).
                // Si el valor es 'false', queremos que esté VISIBLE.
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            // Si el valor no es un booleano, devolvemos Visible como un estado seguro por defecto.
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // La conversión inversa (de Visibilidad a booleano) no es necesaria para nuestro caso de uso.
            // Lanzar esta excepción es la implementación estándar.
            throw new NotImplementedException();
        }
    }
}