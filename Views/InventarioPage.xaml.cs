using AppInventariCor.ViewModels;

namespace AppInventariCor.Views
{
    public partial class InventarioPage : ContentPage
    {
        private InventarioViewModel _viewModel;

        public InventarioPage()
        {
            InitializeComponent();
            _viewModel = new InventarioViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Habilitar aquí cualquier lógica necesaria al aparecer la página
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Limpiar recursos o detener procesos aquí si es necesario
        }
    }
}