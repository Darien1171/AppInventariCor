using System.Globalization;
using Microsoft.Maui.Controls;

namespace AppInventariCor.Converters
{
    public class IntToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Altura base más altura adicional por cada elemento
                return Math.Min(100 + count * 70, 350); // Limitado a 350 para evitar que sea demasiado alto
            }
            return 100; // Altura predeterminada
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}