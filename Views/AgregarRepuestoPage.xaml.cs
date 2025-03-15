using System;
using System.Globalization;
using AppInventariCor.ViewModels;
using Microsoft.Maui.Controls;

namespace AppInventariCor.Views
{
    public partial class AgregarRepuestoPage : ContentPage
    {
        private readonly AgregarRepuestoViewModel _viewModel;

        public AgregarRepuestoPage()
        {
            InitializeComponent();
            _viewModel = new AgregarRepuestoViewModel();
            BindingContext = _viewModel;
        }

        // Método para vincular los campos del formulario con el ViewModel
        private void SetupBindings()
        {
            txtCodigo.BindingContext = _viewModel;
            txtCodigo.SetBinding(Entry.TextProperty, "Codigo");

            txtNombre.BindingContext = _viewModel;
            txtNombre.SetBinding(Entry.TextProperty, "Nombre");

            txtDescripcion.BindingContext = _viewModel;
            txtDescripcion.SetBinding(Editor.TextProperty, "Descripcion");

            txtCategoria.BindingContext = _viewModel;
            txtCategoria.SetBinding(Entry.TextProperty, "Categoria");

            txtMarca.BindingContext = _viewModel;
            txtMarca.SetBinding(Entry.TextProperty, "Marca");

            txtModelo.BindingContext = _viewModel;
            txtModelo.SetBinding(Entry.TextProperty, "Modelo");

            txtUbicacion.BindingContext = _viewModel;
            txtUbicacion.SetBinding(Entry.TextProperty, "Ubicacion");

            txtPrecio.BindingContext = _viewModel;
            txtPrecio.SetBinding(Entry.TextProperty, "Precio", BindingMode.TwoWay, new StringToDecimalConverter());

            txtCantidad.BindingContext = _viewModel;
            txtCantidad.SetBinding(Entry.TextProperty, "Cantidad", BindingMode.TwoWay, new StringToIntConverter());

            txtStockMinimo.BindingContext = _viewModel;
            txtStockMinimo.SetBinding(Entry.TextProperty, "StockMinimo", BindingMode.TwoWay, new StringToIntConverter());

            txtStockOptimo.BindingContext = _viewModel;
            txtStockOptimo.SetBinding(Entry.TextProperty, "StockOptimo", BindingMode.TwoWay, new StringToIntConverter());

            txtCodigoBarras.BindingContext = _viewModel;
            txtCodigoBarras.SetBinding(Entry.TextProperty, "CodigoBarras");

            txtCodigoQR.BindingContext = _viewModel;
            txtCodigoQR.SetBinding(Entry.TextProperty, "CodigoQR");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Establecer bindings al aparecer la página
            SetupBindings();

            // Forzar la actualización de campos para asegurar que se muestren los valores correctos
            if (_viewModel.IsEditMode)
            {
                // Asegurarse de que los valores se muestren correctamente
                txtCodigo.Text = _viewModel.Codigo;
                txtNombre.Text = _viewModel.Nombre;
                txtDescripcion.Text = _viewModel.Descripcion;
                txtCategoria.Text = _viewModel.Categoria;
                txtMarca.Text = _viewModel.Marca;
                txtModelo.Text = _viewModel.Modelo;
                txtUbicacion.Text = _viewModel.Ubicacion;
                txtPrecio.Text = _viewModel.Precio.ToString();
                txtCantidad.Text = _viewModel.Cantidad.ToString();
                txtStockMinimo.Text = _viewModel.StockMinimo.ToString();
                txtStockOptimo.Text = _viewModel.StockOptimo.ToString();
                txtCodigoBarras.Text = _viewModel.CodigoBarras;
                txtCodigoQR.Text = _viewModel.CodigoQR;
            }
        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            bool resultado = await _viewModel.SaveRepuestoAsync();

            if (resultado)
            {
                // Regresar a la página anterior después de guardar exitosamente
                await Shell.Current.GoToAsync("..");
            }
        }

        private async void btnCancelar_Clicked(object sender, EventArgs e)
        {
            // Preguntar si está seguro de cancelar si ha modificado datos
            bool salir = true;

            // Si el usuario ha modificado algún campo, preguntar antes de salir
            if (!string.IsNullOrEmpty(txtNombre.Text) || !string.IsNullOrEmpty(txtCodigo.Text))
            {
                salir = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas cancelar? Los cambios no guardados se perderán.", "Sí", "No");
            }

            if (salir)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    // Convertidores auxiliares para binding
    public class StringToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString(culture);
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (decimal.TryParse(strValue, NumberStyles.Any, culture, out decimal result))
            {
                return result;
            }
            return 0m;
        }
    }

    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (int.TryParse(strValue, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}