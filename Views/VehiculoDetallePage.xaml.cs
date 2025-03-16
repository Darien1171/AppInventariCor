using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class VehiculoDetallePage : ContentPage
    {
        private VehiculoDetalleViewModel viewModel;

        public VehiculoDetallePage()
        {
            InitializeComponent();

            // Crear e inicializar el ViewModel
            viewModel = new VehiculoDetalleViewModel();

            // Asignar el ViewModel como contexto de datos para la página
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Si es necesario, aquí podrías realizar acciones adicionales 
            // cuando la página aparece en pantalla
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Opcionalmente, limpiar recursos o guardar estado
            // cuando la página desaparece de la pantalla
        }
    }
}