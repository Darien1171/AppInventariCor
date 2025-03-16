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

        // Para depuraci�n, podemos a�adir este m�todo para monitorear cambios 
        // en la selecci�n del veh�culo cuando la p�gina aparece
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"[NuevaTransaccionPage] OnAppearing - Estado de selecci�n: " +
                    $"Veh�culo={_viewModel.SelectedVehiculo?.NumeroPlaca ?? "ninguno"}, " +
                    $"Repuesto={_viewModel.SelectedRepuesto?.Nombre ?? "ninguno"}");
            }
        }
    }
}