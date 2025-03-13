using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AppInventariCor.Models; // Añade esta línea para importar el namespace donde está TipoTransaccion

namespace AppInventariCor.Converters
{
    public class AlertLevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string nivelAlerta && !string.IsNullOrEmpty(nivelAlerta))
                {
                    return nivelAlerta switch
                    {
                        "Agotado" => Colors.Red,
                        "Crítico" => Colors.Orange,
                        "Bajo" => Colors.Gold,
                        _ => Colors.Green
                    };
                }
                else if (value is int cantidad)
                {
                    // Asumimos valores bajos como críticos
                    if (cantidad == 0) return Colors.Red;
                    if (cantidad <= 3) return Colors.Orange;
                    if (cantidad <= 5) return Colors.Gold;
                    return Colors.Green;
                }
                return Colors.Green; // Valor por defecto
            }
            catch
            {
                return Colors.Gray; // Valor en caso de error
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TransactionTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string tipo && !string.IsNullOrEmpty(tipo))
                {
                    return tipo switch
                    {
                        "Entrada" => "arrow_downward.png",
                        "Salida" => "arrow_upward.png",
                        "Devolucion" => "undo.png",
                        "Ajuste" => "tune.png",
                        _ => "swap_horiz.png"
                    };
                }
                else if (value is TipoTransaccion tipoEnum)
                {
                    return tipoEnum.ToString() switch
                    {
                        "Entrada" => "arrow_downward.png",
                        "Salida" => "arrow_upward.png",
                        "Devolucion" => "undo.png",
                        "Ajuste" => "tune.png",
                        _ => "swap_horiz.png"
                    };
                }
                return "swap_horiz.png"; // Valor por defecto
            }
            catch
            {
                return "swap_horiz.png"; // Valor en caso de error
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return !string.IsNullOrEmpty(value as string);
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return boolValue;
                }

                // Si no es un bool, intentamos convertirlo
                if (value != null && bool.TryParse(value.ToString(), out bool result))
                {
                    return result;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StockLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Simplificado para evitar dependencias de modelos
                return Colors.Green;
            }
            catch
            {
                return Colors.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return boolValue ? "grid_view.png" : "view_list.png";
                }
                return "view_list.png"; // Valor por defecto
            }
            catch
            {
                return "view_list.png"; // Valor en caso de error
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return boolValue ? 2 : 1;
                }
                return 1; // Valor por defecto
            }
            catch
            {
                return 1; // Valor en caso de error
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return !boolValue;
                }

                // Si no es un bool, intentamos convertirlo
                if (value != null && bool.TryParse(value.ToString(), out bool result))
                {
                    return !result;
                }

                return true; // Por defecto, si no es bool, retornamos true
            }
            catch
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return !boolValue;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}