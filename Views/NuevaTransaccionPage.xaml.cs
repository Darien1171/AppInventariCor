using AppInventariCor.ViewModels;
using AppInventariCor.Models;
using System.Diagnostics;
using Microsoft.Maui.Controls.Compatibility;

namespace AppInventariCor.Views
{
    public partial class NuevaTransaccionPage : ContentPage
    {
        private NuevaTransaccionViewModel _viewModel;

        public NuevaTransaccionPage()
        {
            InitializeComponent();
            _viewModel = new NuevaTransaccionViewModel();
            BindingContext = _viewModel;

            // Suscribirse al evento de cambio de paso
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentStep")
            {
                // Si cambiamos al paso 3, actualizar los valores de cantidad
                if (_viewModel.CurrentStep == 3)
                {
                    // Dar tiempo para que la UI se actualice
                    await Task.Delay(100);
                    ActualizarCantidadesVisibles();
                    ActualizarSubtotales();
                }
            }
        }

        // Nuevo manejador de evento para Entry_Completed
        private void Entry_Completed(object sender, EventArgs e)
        {
            try
            {
                if (sender is Entry entry && int.TryParse(entry.ClassId, out int repuestoId))
                {
                    // Buscar el repuesto en la colección
                    var repuesto = _viewModel.SelectedRepuestos.FirstOrDefault(r => r.Id == repuestoId);

                    if (repuesto != null && int.TryParse(entry.Text, out int cantidad))
                    {
                        _viewModel.UpdateCantidadDirecto(repuesto, cantidad);

                        // Actualizar el subtotal para este ítem
                        ActualizarSubtotalParaRepuesto(repuesto);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Entry_Completed: {ex.Message}");
            }
        }

        // Método para actualizar todos los valores de cantidad visibles
        private void ActualizarCantidadesVisibles()
        {
            try
            {
                // Encontrar todos los Entry de cantidad en el CollectionView
                var entries = this.FindVisualChildren<Entry>().Where(e => e.ClassId != null && e.ClassId != "");

                foreach (var entry in entries)
                {
                    if (int.TryParse(entry.ClassId, out int repuestoId))
                    {
                        // Obtener la cantidad actual del diccionario
                        if (_viewModel.CantidadesRepuestos.TryGetValue(repuestoId, out int cantidad))
                        {
                            entry.Text = cantidad.ToString();
                        }
                        else
                        {
                            entry.Text = "1"; // Valor por defecto
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ActualizarCantidadesVisibles: {ex.Message}");
            }
        }

        // Método para actualizar todos los subtotales
        private void ActualizarSubtotales()
        {
            try
            {
                // Encontrar todos los Label de subtotal
                var labels = this.FindVisualChildren<Label>().Where(l => l.StyleId == "LabelSubtotal");
                var repuestos = _viewModel.SelectedRepuestos.ToList();

                int index = 0;
                foreach (var label in labels)
                {
                    if (index < repuestos.Count)
                    {
                        var repuesto = repuestos[index];
                        ActualizarSubtotalParaRepuesto(repuesto, label);
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ActualizarSubtotales: {ex.Message}");
            }
        }

        // Actualizar el subtotal para un repuesto específico
        private void ActualizarSubtotalParaRepuesto(Repuesto repuesto, Label label = null)
        {
            try
            {
                if (repuesto == null) return;

                // Si no se proporcionó un label, buscar el correspondiente
                if (label == null)
                {
                    var labels = this.FindVisualChildren<Label>().Where(l => l.StyleId == "LabelSubtotal").ToList();
                    var repuestos = _viewModel.SelectedRepuestos.ToList();
                    int index = repuestos.IndexOf(repuesto);

                    if (index >= 0 && index < labels.Count)
                    {
                        label = labels[index];
                    }
                }

                if (label != null && _viewModel.CantidadesRepuestos.TryGetValue(repuesto.Id, out int cantidad))
                {
                    decimal subtotal = repuesto.Precio * cantidad;
                    label.Text = $"Subtotal: ${subtotal:N2}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ActualizarSubtotalParaRepuesto: {ex.Message}");
            }
        }
    }

    // Extensión para encontrar elementos visuales hijos
    public static class VisualElementExtensions
    {
        public static IEnumerable<T> FindVisualChildren<T>(this Element element) where T : Element
        {
            if (element == null)
                yield break;

            if (element is T t)
                yield return t;

            if (element is Layout<View> layout)
            {
                foreach (var child in layout.Children)
                {
                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }

            if (element is ContentView contentView && contentView.Content != null)
            {
                foreach (var childOfContent in FindVisualChildren<T>(contentView.Content))
                {
                    yield return childOfContent;
                }
            }

            if (element is ContentPage contentPage && contentPage.Content != null)
            {
                foreach (var childOfPage in FindVisualChildren<T>(contentPage.Content))
                {
                    yield return childOfPage;
                }
            }
        }
    }
}