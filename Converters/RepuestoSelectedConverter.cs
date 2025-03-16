using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using AppInventariCor.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AppInventariCor.Converters
{
    public class RepuestoSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            // Verificar si es un botón que necesita texto
            if (targetType == typeof(string))
            {
                bool isSelected = IsRepuestoSelected(value, parameter);
                return isSelected ? "Quitar" : "Agregar";
            }

            // Verificar si se usa para color del botón
            if (parameter is string paramStr && paramStr == "ButtonColor")
            {
                bool isSelected = IsRepuestoSelected(value, parameter);
                return isSelected ? Colors.Orange : Colors.Green;
            }

            // Caso predeterminado - resultado booleano para indicar si está seleccionado
            return IsRepuestoSelected(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private bool IsRepuestoSelected(object repuesto, object selectedRepuestos)
        {
            if (repuesto is Repuesto currentRepuesto &&
                selectedRepuestos is ObservableCollection<Repuesto> collection)
            {
                return collection.Any(r => r.Id == currentRepuesto.Id);
            }

            return false;
        }
    }
}