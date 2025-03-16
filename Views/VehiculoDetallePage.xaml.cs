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

            // Asignar el ViewModel como contexto de datos para la p�gina
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Si es necesario, aqu� podr�as realizar acciones adicionales 
            // cuando la p�gina aparece en pantalla
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Opcionalmente, limpiar recursos o guardar estado
            // cuando la p�gina desaparece de la pantalla
        }
    }
}