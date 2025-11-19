using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ATMScoreBoard.Display.Converters
{
    // Este convertidor traduce un valor booleano a su valor de Visibilidad DIRECTO.
    // true -> Visible
    // false -> Collapsed
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Si el valor es 'true', queremos que esté VISIBLE.
                // Si el valor es 'false', queremos que esté OCULTO (Collapsed).
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed; // Estado seguro por defecto
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}