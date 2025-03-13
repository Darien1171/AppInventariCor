using AppInventariCor.Models;
using AppInventariCor.ViewModels;
using System.Diagnostics;

namespace AppInventariCor.Views
{
    public partial class RepuestoDetallePage : ContentPage
    {
        private RepuestoDetalleViewModel _viewModel;

        public RepuestoDetallePage()
        {
            try
            {
                InitializeComponent();

                // Crear un repuesto de ejemplo detallado
                var repuesto = new Repuesto
                {
                    Id = 999,
                    Codigo = "REP-DEMO",
                    Nombre = "Repuesto de Demostraci�n",
                    Descripcion = "Este es un repuesto de demostraci�n con informaci�n completa para probar la p�gina de detalles.",
                    Categoria = "Demostraci�n",
                    Marca = "TestMarca",
                    Modelo = "TestModelo",
                    Precio = 99.99m,
                    Cantidad = 10,
                    StockMinimo = 5,
                    StockOptimo = 20,
                    Ubicacion = "Almac�n Central",
                    CodigoBarras = "7501234567890",
                    CodigoQR = "DEMO-QR-CODE-12345"
                };

                // Crear el ViewModel y asignar el repuesto
                _viewModel = new RepuestoDetalleViewModel(repuesto);

                // Establecer el BindingContext de la p�gina
                BindingContext = _viewModel;

                Debug.WriteLine("RepuestoDetallePage inicializada correctamente con repuesto de demostraci�n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en constructor de RepuestoDetallePage: {ex}");
                DisplayAlert("Error", $"Error al inicializar la p�gina: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("RepuestoDetallePage: OnAppearing llamado");

            // Verificar si el BindingContext est� configurado correctamente
            if (BindingContext is RepuestoDetalleViewModel viewModel)
            {
                Debug.WriteLine($"ViewModel actual: {viewModel.GetType().Name}");
                Debug.WriteLine($"Repuesto en ViewModel: {viewModel.Repuesto?.Nombre ?? "No establecido"}");
            }
            else
            {
                Debug.WriteLine($"BindingContext no es RepuestoDetalleViewModel: {BindingContext?.GetType().Name ?? "null"}");
            }
        }
    }
}