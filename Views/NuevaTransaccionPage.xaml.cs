using AppInventariCor.ViewModels;

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
        }

        // Para depuración, podemos añadir este método para monitorear cambios 
        // en la selección del vehículo cuando la página aparece
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"[NuevaTransaccionPage] OnAppearing - Estado de selección: " +
                    $"Vehículo={_viewModel.SelectedVehiculo?.NumeroPlaca ?? "ninguno"}, " +
                    $"Repuesto={_viewModel.SelectedRepuesto?.Nombre ?? "ninguno"}");
            }
        }
    }
}