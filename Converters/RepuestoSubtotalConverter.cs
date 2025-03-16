using AppInventariCor.Models;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AppInventariCor.Converters
{
    public class RepuestoSubtotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Repuesto repuesto && parameter is Dictionary<int, int> cantidades)
            {
                if (cantidades.TryGetValue(repuesto.Id, out int cantidad))
                {
                    decimal subtotal = repuesto.Precio * cantidad;
                    return $"Subtotal: ${subtotal:N2}";
                }
            }
            return "Subtotal: $0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}